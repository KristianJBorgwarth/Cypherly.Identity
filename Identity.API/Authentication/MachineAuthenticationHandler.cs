using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Identity.API.Authentication;

public sealed class MachineAuthenticationHandler(
    IOptionsMonitor<MachineAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<MachineAuthenticationOptions>(options, logger, encoder)
{
    private const string ApiKeyHeader = "x-api-key";
    
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if(!Request.Headers.TryGetValue(ApiKeyHeader , out var providedApiKey) || string.IsNullOrWhiteSpace(providedApiKey))
            return Task.FromResult(AuthenticateResult.Fail("API Key was not provided."));
        
        if (!string.Equals(providedApiKey, Options.Key, StringComparison.Ordinal))
            return Task.FromResult(AuthenticateResult.Fail("API Key is not valid."));

        var claims = new[] { new System.Security.Claims.Claim("Machine", "True") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, Scheme.Name);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}