using System.Security.Claims;
using System.Text;
using Identity.Application.Extensions;
using Identity.Application.Features.Authentication.Queries.GetJwks;
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
    public string GenerateToken(Guid userId, Guid deviceId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("device_id", deviceId.ToString()),
            new("jti", Guid.NewGuid().ToString()),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new(claims),
            Expires = DateTime.UtcNow.AddDays(jwtSettings.Value.TokenLifeTimeInMinutes),
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
        var jwks = await jwkCache.GetJwks();
        var jwksDtos = jwks.ToPubKeyDtos();
        return jwksDtos;
    }
}
