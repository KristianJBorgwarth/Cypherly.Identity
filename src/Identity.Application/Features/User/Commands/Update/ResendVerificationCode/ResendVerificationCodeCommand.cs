using Identity.Application.Abstractions;
using Identity.Domain.Enums;

namespace Identity.Application.Features.User.Commands.Update.ResendVerificationCode;

public sealed record ResendVerificationCodeCommand : ICommand
{
    public required Guid UserId { get; init; }
    public required UserVerificationCodeType CodeType { get; init; }
}