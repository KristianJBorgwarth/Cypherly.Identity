using Cypherly.Domain.Common;
using Identity.Domain.Common;
using FluentValidation;
using Identity.Application.Features.User.Commands.Update.Verify;

namespace Identity.Application.Features.Authentication.Commands.VerifyLogin;

public sealed class VerifyLoginCommandValidator : AbstractValidator<VerifyLoginCommand>
{
    public VerifyLoginCommandValidator()
    {
        RuleFor(cmd => cmd.UserId)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(VerifyUserCommand.UserId)).Message);

        RuleFor(cmd => cmd.LoginVerificationCode)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(VerifyUserCommand.VerificationCode)).Message);
    }
}