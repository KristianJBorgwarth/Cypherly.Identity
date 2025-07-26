
namespace Cypherly.Identity.Application.Features.Authentication.Token;

public class JwtSettings
{
    public string Secret { get; init; } = null!;
    public string Issuer { get; init; } = null!;
    public string Audience { get; init; } = null!;
    public int TokenLifeTimeInMinutes { get; init; }
}