using System.Security.Cryptography;
using Identity.Application.Contracts.RateLimiting;
using Identity.Application.Contracts.Repository;
using Identity.Application.Interfaces;
using Microsoft.Extensions.Logging;
using PrivacyPass.Issuance;

namespace Identity.Application.Services;

internal sealed class TokenIssuer(
    ITokenIssuerKeyRepository tokenIssuerKeyRepository,
    IRateLimiter rateLimiter,
    ILogger<TokenIssuer> logger)
    : ITokenIssuer
{
    public const int MaxTokenRequests = 3;

    private static readonly TimeSpan IssuanceWindow = TimeSpan.FromHours(24);
    private const int IssuanceCount = 4;

    public async Task<IReadOnlyList<string>> IssueAsync(Guid userId, IReadOnlyList<string> tokenRequests, CancellationToken ct = default)
    {
        if (tokenRequests.Count is 0) return [];

        try
        {
            var key = await tokenIssuerKeyRepository.GetCurrentAsync(ct);
            if (key is null)
            {
                logger.LogWarning("No token issuer key exists yet, so no tokens were issued to user {UserId}", userId);
                return [];
            }

            if (!await rateLimiter.IsAllowedAsync($"tokens:issue:{userId}", IssuanceCount, IssuanceWindow, ct))
            {
                logger.LogInformation("User {UserId} reached the token issuance limit", userId);
                return [];
            }

            using var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(key.PrivateKey, out _);
            var signer = new BlindSigner(rsa);

            var tokenResponses = new List<string>(tokenRequests.Count);

            foreach (var encoded in tokenRequests)
            {
                if (!TryDecode(encoded, out var bytes) || !TokenRequest.TryParse(bytes, out var tokenRequest))
                {
                    logger.LogWarning("User {UserId} sent a malformed token request, so no tokens were issued", userId);
                    return [];
                }

                tokenResponses.Add(Convert.ToBase64String(signer.Sign(tokenRequest!).Encode()));
            }

            return tokenResponses;
        }
        catch (CryptographicException ex)
        {
            logger.LogWarning(ex, "Token requests from user {UserId} could not be signed", userId);
            return [];
        }
    }

    public async Task<byte[]?> GetPublicKeyAsync(CancellationToken ct = default)
    {
        var key = await tokenIssuerKeyRepository.GetCurrentAsync(ct);
        return key?.PublicKey;
    }

    private static bool TryDecode(string encoded, out byte[] bytes)
    {
        var buffer = new byte[encoded.Length];
        var decoded = Convert.TryFromBase64String(encoded, buffer, out var written);
        bytes = buffer[..written];
        return decoded;
    }
}
