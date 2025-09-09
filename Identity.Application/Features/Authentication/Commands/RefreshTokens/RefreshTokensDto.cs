using Identity.Domain.Entities;

namespace Identity.Application.Features.Authentication.Commands.RefreshTokens;

public sealed record RefreshTokensDto
{
    public required string Jwt { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
}