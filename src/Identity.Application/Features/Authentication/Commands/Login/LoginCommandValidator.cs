using Cendrite.Domain.Common;
using Identity.Domain.Common;
using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotNull().WithMessage(Errors.General.ValueIsRequired(nameof(LoginCommand.Email)).Description)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(LoginCommand.Email)).Description)
            .Must(email => email.Length <= 255).WithMessage(Errors.General.ValueTooLarge(nameof(LoginCommand.Email), 255).Description);

        RuleFor(x => x.Password)
            .NotNull().WithMessage(Errors.General.ValueIsRequired(nameof(LoginCommand.Password)).Description)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(LoginCommand.Password)).Description)
            .Must(pw => pw.Length <= 255).WithMessage(Errors.General.ValueTooLarge(nameof(LoginCommand.Password), 255).Description);

    }
}