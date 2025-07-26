using Cypherly.Domain.Common;
using Cypherly.Identity.Domain.Common;
using FluentValidation;

namespace Cypherly.Identity.Application.Features.Authentication.Commands.RefreshTokens;

public class RefreshTokensCommandValidator : AbstractValidator<RefreshTokensCommand>
{
    public RefreshTokensCommandValidator()
    {
        RuleFor(p => p.RefreshToken)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(RefreshTokensCommand.RefreshToken)).Message);

        RuleFor(p => p.UserId)
            .NotEmpty().
            WithMessage(Errors.General.ValueIsEmpty(nameof(RefreshTokensCommand.UserId)).Message);

        RuleFor(p => p.DeviceId)
            .NotEmpty().
            WithMessage(Errors.General.ValueIsEmpty(nameof(RefreshTokensCommand.DeviceId)).Message);
    }
}