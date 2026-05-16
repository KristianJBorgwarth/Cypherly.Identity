namespace Identity.Application.Settings;

public class JwtSettings
{
    public required string Authority { get; init; } 
    public required string Audience { get; init; } 
    public required string TokenEndpoint { get; init; } 
    public required int TokenLifeTimeInMinutes { get; init; }
}
