using Identity.Application.Dtos;

namespace Identity.Application.Features.Authentication.Queries.GetJwks;

public sealed record JwksResponse
{
    public required IReadOnlyList<JwksDto> Keys { get; init; } 
}