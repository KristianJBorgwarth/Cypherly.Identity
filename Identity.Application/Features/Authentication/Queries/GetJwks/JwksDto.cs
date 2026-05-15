namespace Identity.Application.Features.Authentication.Queries.GetJwks;

public sealed record JwksDto
{
    public required string Kty { get; init; } = "RSA";
    public required string Use { get; init; } = "sig";
    public required string Kid { get; init; }
    public required string N { get; init; }
    public required string E { get; init; }
}
