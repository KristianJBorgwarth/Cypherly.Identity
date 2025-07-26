using Identity.Application.Abstractions;

namespace Identity.Application.Features.User.Commands.Update.Verify;

public sealed record VerifyUserCommand : ICommand
{
    public required Guid UserId { get; init; }
    public required string VerificationCode { get; init; }
}