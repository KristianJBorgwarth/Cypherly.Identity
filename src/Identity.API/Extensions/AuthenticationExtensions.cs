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
                options.RequireHttpsMetadata = false;
                options.Authority = jwtSettings.Issuer;
                options.Audience = jwtSettings.Audience;

                // Default is 12 hours. Must stay under the key rotation interval.
                options.AutomaticRefreshInterval = TimeSpan.FromMinutes(2);

                // Floor on how often an unknown kid can force an early metadata refetch.
                options.RefreshInterval = TimeSpan.FromMinutes(1);
                options.RefreshOnIssuerKeyNotFound = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,

                    // Defaults to 5 minutes, which the retirement grace period has to cover.
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });

        services.AddAuthorization();
    }
}
