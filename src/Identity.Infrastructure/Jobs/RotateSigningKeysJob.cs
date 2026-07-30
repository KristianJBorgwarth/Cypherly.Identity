using Identity.Application.Contracts.Repository;
using Identity.Domain.Aggregates;
using Identity.Infrastructure.Interfaces;
using Identity.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Identity.Infrastructure.Jobs;

/// <summary>
/// Creates the first signing key, replaces it once it reaches its rotation interval, and
/// deletes retired keys once no token they signed can still be valid.
/// <para>
/// Safe to run on every replica: the persistent job store enforces
/// <see cref="DisallowConcurrentExecutionAttribute"/> across the cluster, and the partial
/// unique index on is_current is the backstop if scheduling ever fails to.
/// </para>
/// </summary>
[DisallowConcurrentExecution]
internal sealed class RotateSigningKeysJob(
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

    public async Task Execute(IJobExecutionContext jobContext)
    {
        var ct = jobContext.CancellationToken;

        try
        {
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

            // One SaveChanges, so retiring the old key and inserting its replacement commit together.
            await unitOfWork.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rotating signing keys");
        }
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
