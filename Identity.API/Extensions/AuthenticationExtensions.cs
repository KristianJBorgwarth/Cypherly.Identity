using Identity.Application.Settings;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Extensions;

internal static class AuthenticationExtensions
{
    public static void AddAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>() ?? throw new InvalidOperationException("Jwt settings are not configured properly.");

        services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = jwtSettings.Authority;
                options.Audience = jwtSettings.Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Authority,
                    ValidAudience = jwtSettings.Audience,
                };
            });

        services.AddAuthorization();
    }
}
