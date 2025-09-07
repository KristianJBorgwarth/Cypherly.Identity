using System.Text;
using Identity.Application.Features.Authentication.Token;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Extensions;

internal static class AuthenticationExtensions
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings is null) throw new InvalidOperationException("Jwt settings are not configured properly.");

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer ??
                                  throw new NotImplementedException(
                                      $"MISSING VALUE IN JWT SETTINGS {jwtSettings.Issuer}"),
                    ValidAudience = jwtSettings.Audience ??
                                    throw new NotImplementedException(
                                        $"MISSING VALUE IN JWT SETTINGS {jwtSettings.Audience}"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        services.AddAuthorization();
    }
}