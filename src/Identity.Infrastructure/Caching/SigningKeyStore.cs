using System.Security.Cryptography;
using Identity.Application.Contracts.Repository;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos;
using Identity.Domain.Aggregates;
using Identity.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Caching;

/// <summary>
/// Holds an in-process snapshot of the signing keys so neither token issuance nor the
/// public JWKS endpoint hits the database per request.
/// <para>
/// Deliberately not distributed. Private key material must never reach Valkey, and the
/// public half is a handful of rows that every replica can afford to read for itself.
/// Replicas are briefly inconsistent after a rotation; the retirement grace period is
/// what makes that harmless, so no cross-process invalidation is needed.
/// </para>
/// </summary>
internal sealed class SigningKeyStore(
    IServiceScopeFactory scopeFactory,
    IOptions<SigningKeySettings> settings,
    TimeProvider timeProvider,
    ILogger<SigningKeyStore> logger)
    : ISigningKeyProvider
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Snapshot? _snapshot;

    public async Task<ActiveSigningKey> GetCurrentAsync(CancellationToken ct = default)
        => (await GetSnapshotAsync(ct)).Current;

    public async Task<IReadOnlyList<JwksDto>> GetPublishedAsync(CancellationToken ct = default)
        => (await GetSnapshotAsync(ct)).Published;

    private async Task<Snapshot> GetSnapshotAsync(CancellationToken ct)
    {
        var fresh = Volatile.Read(ref _snapshot);
        if (IsFresh(fresh)) return fresh!;

        await _gate.WaitAsync(ct);
        try
        {
            var stale = Volatile.Read(ref _snapshot);
            if (IsFresh(stale)) return stale!;

            try
            {
                var loaded = await LoadAsync(ct);
                Volatile.Write(ref _snapshot, loaded);
                return loaded;
            }
            catch (Exception ex) when (stale is not null)
            {
                // A database blip must not stop us issuing or validating tokens.
                logger.LogError(ex,
                    "Failed to reload signing keys. Serving the snapshot loaded at {LoadedAt:o}.", stale.LoadedAt);
                return stale;
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsFresh(Snapshot? snapshot)
        => snapshot is not null && timeProvider.GetUtcNow() - snapshot.LoadedAt < settings.Value.CacheTtl;

    private async Task<Snapshot> LoadAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISigningKeyRepository>();

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var keys = await repository.GetPublishedAsync(now, ct);

        var current = keys.SingleOrDefault(k => k.IsCurrent)
                      ?? throw new InvalidOperationException(
                          "No current signing key exists. The rotation job creates one on startup.");

        var published = keys
            .Select(k => new JwksDto
            {
                Kid = k.Kid,
                N = k.N,
                E = k.E,
                Kty = k.Kty,
                Use = k.Use,
                Alg = k.Alg,
            })
            .ToList();

        return new Snapshot(ToSigningKey(current), published, timeProvider.GetUtcNow());
    }

    private static ActiveSigningKey ToSigningKey(SigningKey key)
    {
        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(key.PrivateKey, out _);

        // Copy the parameters out so the snapshot holds no undisposed RSA handle.
        var securityKey = new RsaSecurityKey(rsa.ExportParameters(true)) { KeyId = key.Kid };
        return new ActiveSigningKey(key.Kid, key.Alg, securityKey);
    }

    private sealed record Snapshot(
        ActiveSigningKey Current,
        IReadOnlyList<JwksDto> Published,
        DateTimeOffset LoadedAt);
}
