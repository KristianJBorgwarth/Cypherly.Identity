namespace Identity.Application.Features.Authentication.Queries.GetJwks;

public sealed record JwksDto
{
    public string Kty { get; } = "RSA";
    public string Use { get; } = "sig";
    public string Alg { get; } = "RS256";
    public required string Kid { get; init; }
    public required string N { get; init; }
    public required string E { get; init; }
}
