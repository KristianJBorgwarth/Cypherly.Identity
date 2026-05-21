using FluentValidation;

namespace Identity.Application.Features.User.Commands.Update.Verify;

public class VerifyUserCommandValidator : AbstractValidator<VerifyUserCommand>
{
    public VerifyUserCommandValidator()
    {
        RuleFor(cmd => cmd.UserId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(VerifyUserCommand.UserId)} ");

        RuleFor(cmd => cmd.VerificationCode)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(VerifyUserCommand.VerificationCode)} ");
    }
}
