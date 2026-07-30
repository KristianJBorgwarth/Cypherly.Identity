using System.Security.Claims;
using System.Security.Cryptography;
using Identity.Application.Contracts.Repository;
using Identity.Application.Dtos;
using Identity.Application.Interfaces;
using Identity.Application.Settings;
using Identity.Domain.Aggregates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Services;

internal class JwtService(
    ISigningKeyRepository signingKeyRepository,
    TimeProvider timeProvider,
    IOptions<JwtSettings> jwtSettings)
    : IJwtService
{
    public async Task<string> GenerateTokenAsync(Guid userId, Guid deviceId, CancellationToken ct = default)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var signingKey = await GetCurrentKeyAsync(now, ct);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("device_id", deviceId.ToString()),
            new("jti", Guid.NewGuid().ToString()),
        };

        // The key carries its own algorithm, and its kid goes into the header so
        // validators know which JWKS entry to verify against.
        var creds = new SigningCredentials(ToSecurityKey(signingKey), signingKey.Alg);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new(claims),
            Expires = now.AddMinutes(jwtSettings.Value.TokenLifeTimeInMinutes),
            SigningCredentials = creds,
            Issuer = jwtSettings.Value.Issuer,
            Audience = jwtSettings.Value.Audience,
        };

        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return token;
    }

    public async Task<IReadOnlyList<JwksDto>> GenerateJwks(CancellationToken ct = default)
    {
        var keys = await signingKeyRepository.GetPublishedAsync(timeProvider.GetUtcNow().UtcDateTime, ct);

        return keys
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
    }

    /// <summary>
    /// Reads through the published set rather than <see cref="ISigningKeyRepository.GetCurrentAsync"/>,
    /// which returns a tracked entity for the rotation job's benefit. Nothing here mutates the key,
    /// and private key material has no business in the request's change tracker.
    /// </summary>
    private async Task<SigningKey> GetCurrentKeyAsync(DateTime now, CancellationToken ct)
    {
        var keys = await signingKeyRepository.GetPublishedAsync(now, ct);

        return keys.SingleOrDefault(k => k.IsCurrent)
               ?? throw new InvalidOperationException(
                   "No current signing key exists. The rotation job creates one on startup.");
    }

    private static RsaSecurityKey ToSecurityKey(SigningKey key)
    {
        using var rsa = RSA.Create();
        rsa.ImportPkcs8PrivateKey(key.PrivateKey, out _);

        // Copy the parameters out so the returned key holds no undisposed RSA handle.
        return new RsaSecurityKey(rsa.ExportParameters(true)) { KeyId = key.Kid };
    }
}
