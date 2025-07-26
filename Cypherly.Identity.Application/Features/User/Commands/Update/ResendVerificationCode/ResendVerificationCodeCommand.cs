using Cypherly.Identity.Application.Abstractions;
using Cypherly.Identity.Domain.Enums;

namespace Cypherly.Identity.Application.Features.User.Commands.Update.ResendVerificationCode;

public sealed record ResendVerificationCodeCommand : ICommand
{
    public required Guid UserId { get; init; }
    public required UserVerificationCodeType CodeType { get; init; }
}