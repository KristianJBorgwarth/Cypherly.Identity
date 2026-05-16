using System.Security.Claims;
using Identity.Application.Dtos;
using Identity.Application.Extensions;
using Identity.Application.Interfaces;
using Identity.Application.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Application.Services;

internal class JwtService(
    IJwkCache jwkCache,
    IOptions<JwtSettings> jwtSettings)
    : IJwtService
{
    public async Task<string> GenerateTokenAsync(Guid userId, Guid deviceId, CancellationToken ct = default)
    {
        var jwks = await jwkCache.GetJwks(ct);
        var signingKey = jwks.GetSigningKeys().FirstOrDefault() ?? throw new InvalidOperationException("No signing key available.");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("device_id", deviceId.ToString()),
            new("jti", Guid.NewGuid().ToString()),
        };

        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new(claims),
            Expires = DateTime.UtcNow.AddMinutes(jwtSettings.Value.TokenLifeTimeInMinutes),
            SigningCredentials = creds,
            Issuer = jwtSettings.Value.Authority,
            Audience = jwtSettings.Value.Audience,
        };

        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return token;
    }


    public async Task<IReadOnlyList<JwksDto>> GenerateJwks(CancellationToken ct = default)
    {
        var jwks = await jwkCache.GetJwks(ct);
        var jwksDtos = jwks.ToPubKeyDtos();
        return jwksDtos;
    }
}
