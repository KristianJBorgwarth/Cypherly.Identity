using System.Security.Claims;
using Identity.Application.Contracts.Security;
using Identity.Application.Dtos;
using Identity.Application.Interfaces;
using Identity.Application.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Services;

internal class JwtService(
    ISigningKeyProvider signingKeyProvider,
    IOptions<JwtSettings> jwtSettings)
    : IJwtService
{
    public async Task<string> GenerateTokenAsync(Guid userId, Guid deviceId, CancellationToken ct = default)
    {
        var signingKey = await signingKeyProvider.GetCurrentAsync(ct);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("device_id", deviceId.ToString()),
            new("jti", Guid.NewGuid().ToString()),
        };

        // The key carries its own algorithm, and its kid goes into the header so
        // validators know which JWKS entry to verify against.
        var creds = new SigningCredentials(signingKey.Key, signingKey.Alg);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.Value.TokenLifeTimeInMinutes),
            SigningCredentials = creds,
            Issuer = jwtSettings.Value.Issuer,
            Audience = jwtSettings.Value.Audience,
        };

        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return token;
    }

    public Task<IReadOnlyList<JwksDto>> GenerateJwks(CancellationToken ct = default)
        => signingKeyProvider.GetPublishedAsync(ct);
}
