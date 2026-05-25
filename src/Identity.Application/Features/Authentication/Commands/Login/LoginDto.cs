namespace Identity.Application.Features.Authentication.Commands.Login;

public sealed record LoginDto
{
    public required Guid UserId { get; init; }
    public required bool IsVerified { get; init; }
}