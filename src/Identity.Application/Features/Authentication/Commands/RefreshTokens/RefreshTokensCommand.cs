using Identity.Application.Abstractions;

namespace Identity.Application.Features.Authentication.Commands.RefreshTokens;

public sealed record RefreshTokensCommand : ICommand<RefreshTokensDto>
{
    public Guid UserId { get; init; }
    public Guid DeviceId { get; init; }
    public required string RefreshToken { get; init; }
}