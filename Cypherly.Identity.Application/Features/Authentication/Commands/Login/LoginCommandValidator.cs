using Cypherly.Domain.Common;
using Cypherly.Identity.Domain.Common;
using FluentValidation;

namespace Cypherly.Identity.Application.Features.Authentication.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotNull().WithMessage(Errors.General.ValueIsRequired(nameof(LoginCommand.Email)).Message)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(LoginCommand.Email)).Message)
            .Must(email => email.Length <= 255).WithMessage(Errors.General.ValueTooLarge(nameof(LoginCommand.Email), 255).Message);

        RuleFor(x => x.Password)
            .NotNull().WithMessage(Errors.General.ValueIsRequired(nameof(LoginCommand.Password)).Message)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(LoginCommand.Password)).Message)
            .Must(pw => pw.Length <= 255).WithMessage(Errors.General.ValueTooLarge(nameof(LoginCommand.Password), 255).Message);

    }
}