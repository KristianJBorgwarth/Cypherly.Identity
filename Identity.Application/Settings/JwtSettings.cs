namespace Identity.Application.Settings;

public class JwtSettings
{
    public required string Issuer { get; init; } 
    public required string Audience { get; init; } 
    public required int TokenLifeTimeInMinutes { get; init; }
}
