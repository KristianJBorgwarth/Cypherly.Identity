using Cypherly.Domain.Common;
using Identity.Domain.Common;
using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.RefreshTokens;

public class RefreshTokensCommandValidator : AbstractValidator<RefreshTokensCommand>
{
    public RefreshTokensCommandValidator()
    {
        RuleFor(p => p.RefreshToken)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(RefreshTokensCommand.RefreshToken)).Description);

        RuleFor(p => p.UserId)
            .NotEmpty().
            WithMessage(Errors.General.ValueIsEmpty(nameof(RefreshTokensCommand.UserId)).Description);

        RuleFor(p => p.DeviceId)
            .NotEmpty().
            WithMessage(Errors.General.ValueIsEmpty(nameof(RefreshTokensCommand.DeviceId)).Description);
    }
}