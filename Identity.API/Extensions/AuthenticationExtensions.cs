using System.Text;
using Identity.API.Authentication;
using Identity.Application.Features.Authentication.Token;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Extensions;

internal static class AuthenticationExtensions
{
    public static void AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings is null) throw new InvalidOperationException("Jwt settings are not configured properly.");
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Jwt";
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer ?? throw new NotImplementedException($"MISSING VALUE IN JWT SETTINGS {jwtSettings.Issuer}"),
                    ValidAudience = jwtSettings.Audience ?? throw new NotImplementedException($"MISSING VALUE IN JWT SETTINGS {jwtSettings.Audience}"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            })
            .AddScheme<MachineAuthenticationOptions, MachineAuthenticationHandler>(
                "Machine", 
                options =>
                {
                    var key = configuration["MachineAuthentication:Key"];
                    if (string.IsNullOrEmpty(key)) throw new InvalidOperationException("Machine authentication key is not configured properly.");
                    options.Key = key;
                });
        
        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.Machine, policy =>
            {
                policy.AddAuthenticationSchemes("Machine");
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("Machine", "True");
            });
    }
}