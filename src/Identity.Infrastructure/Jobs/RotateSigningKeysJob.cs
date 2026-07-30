using Identity.Application.Contracts.Repository;
using Identity.Domain.Aggregates;
using Identity.Infrastructure.Interfaces;
using Identity.Infrastructure.Persistence.Context;
using Identity.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Identity.Infrastructure.Jobs;

[DisallowConcurrentExecution]
internal sealed class RotateSigningKeysJob(
    IdentityDbContext context,
    ISigningKeyRepository signingKeyRepository,
    IRsaKeyGenerator rsaKeyGenerator,
    IUnitOfWork unitOfWork,
    IOptions<SigningKeySettings> settings,
    TimeProvider timeProvider,
    ILogger<RotateSigningKeysJob> logger)
    : IJob
{
    private const string Kty = "RSA";
    private const string Use = "sig";
    private const string Alg = "RS256";

    /// <summary>
    /// Quartz runs in-memory, so every replica fires this job. The advisory lock — not
    /// [DisallowConcurrentExecution], which is per-process — is what stops two replicas
    /// rotating at once.
    /// </summary>
    private const long AdvisoryLockId = 0x5347_4E4B; // "SGNK"

    public async Task Execute(IJobExecutionContext jobContext)
    {
        var ct = jobContext.CancellationToken;

        try
        {
            // EnableRetryOnFailure means user-initiated transactions must go through the strategy.
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () => await RotateAsync(ct));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rotating signing keys");
        }
    }

    private async Task RotateAsync(CancellationToken ct)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);

        await context.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock({0})", [AdvisoryLockId], ct);

        var options = settings.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var current = await signingKeyRepository.GetCurrentAsync(ct);

        if (current is null)
        {
            var created = await CreateKeyAsync(ct);
            logger.LogInformation("No signing key existed. Created {Kid}.", created.Kid);
        }
        else if (now - current.Created >= options.RotationInterval)
        {
            current.Retire(now, options.RetirementGracePeriod);
            var created = await CreateKeyAsync(ct);

            logger.LogInformation(
                "Rotated signing key {OldKid} to {NewKid}. {OldKid} stays in the JWKS until {ExpiresAt:o}.",
                current.Kid, created.Kid, current.Kid, current.ExpiresAt);
        }

        foreach (var purgeable in await signingKeyRepository.GetPurgeableAsync(now, ct))
        {
            signingKeyRepository.Delete(purgeable);
            logger.LogInformation(
                "Purged signing key {Kid}, which left the JWKS at {ExpiresAt:o}.", purgeable.Kid, purgeable.ExpiresAt);
        }

        await unitOfWork.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    private async Task<SigningKey> CreateKeyAsync(CancellationToken ct)
    {
        var generated = rsaKeyGenerator.GenerateKey(settings.Value.KeySize);

        var key = new SigningKey(
            Guid.NewGuid(),
            generated.Kid,
            generated.N,
            generated.E,
            Kty,
            Use,
            Alg,
            generated.Pkcs8PrivateKey);

        await signingKeyRepository.CreateAsync(key, ct);
        return key;
    }
}
