using FluentValidation;

namespace Identity.Application.Features.User.Commands.Update.ResendVerificationCode;

public class ResendVerificationCodeCommandValidator : AbstractValidator<ResendVerificationCodeCommand>
{
    public ResendVerificationCodeCommandValidator()
    {
        RuleFor(cmd => cmd.UserId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(ResendVerificationCodeCommand.UserId)} ");

        RuleFor(cmd => cmd.CodeType)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(ResendVerificationCodeCommand.CodeType)} ")
            .IsInEnum().WithMessage($"Value '{nameof(ResendVerificationCodeCommand.CodeType)}' is not valid in this context");
    }
}
