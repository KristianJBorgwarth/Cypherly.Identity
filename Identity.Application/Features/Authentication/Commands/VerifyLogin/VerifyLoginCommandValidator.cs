using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.VerifyLogin;

public sealed class VerifyLoginCommandValidator : AbstractValidator<VerifyLoginCommand>
{
    public VerifyLoginCommandValidator()
    {
        RuleFor(cmd => cmd.UserId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(VerifyLoginCommand.UserId)} ");

        RuleFor(cmd => cmd.LoginVerificationCode)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(VerifyLoginCommand.LoginVerificationCode)} ");
    }
}
