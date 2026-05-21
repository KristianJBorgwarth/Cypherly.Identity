using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotNull().WithMessage($"Value '{nameof(LoginCommand.Email)}' is required.")
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(LoginCommand.Email)} ")
            .Must(email => email.Length <= 255).WithMessage($"Value '{nameof(LoginCommand.Email)}' should not exceed 255.");

        RuleFor(x => x.Password)
            .NotNull().WithMessage($"Value '{nameof(LoginCommand.Password)}' is required.")
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(LoginCommand.Password)} ")
            .Must(pw => pw.Length <= 255).WithMessage($"Value '{nameof(LoginCommand.Password)}' should not exceed 255.");
    }
}
