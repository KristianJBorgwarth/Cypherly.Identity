using System.Security.Cryptography;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Aggregates;
using Microsoft.Extensions.Logging;
using PrivacyPass.Keys;
using PrivacyPass.Tokens;
using Quartz;

namespace Identity.Infrastructure.Jobs;

[DisallowConcurrentExecution]
internal sealed class EnsureTokenIssuerKeyJob(
    ITokenIssuerKeyRepository tokenIssuerKeyRepository,
    IUnitOfWork unitOfWork,
    ILogger<EnsureTokenIssuerKeyJob> logger)
    : IJob
{
    public async Task Execute(IJobExecutionContext jobContext)
    {
        var ct = jobContext.CancellationToken;

        try
        {
            if (await tokenIssuerKeyRepository.GetCurrentAsync(ct) is not null) return;

            using var rsa = RSA.Create(TokenType.KeySizeInBits);
            var key = new TokenIssuerKey(Guid.NewGuid(), TokenKeyId.EncodePublicKey(rsa), rsa.ExportPkcs8PrivateKey());

            await tokenIssuerKeyRepository.CreateAsync(key, ct);
            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("No token issuer key existed. Created {KeyId}.", key.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring a token issuer key");
        }
    }
}
