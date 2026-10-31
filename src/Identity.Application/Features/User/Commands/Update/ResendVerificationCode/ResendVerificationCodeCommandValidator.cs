using Cendrite.Domain.Common;
using Identity.Domain.Common;
using FluentValidation;

namespace Identity.Application.Features.User.Commands.Update.ResendVerificationCode;

public class ResendVerificationCodeCommandValidator : AbstractValidator<ResendVerificationCodeCommand>
{
    public ResendVerificationCodeCommandValidator()
    {
        RuleFor(cmd => cmd.UserId)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(ResendVerificationCodeCommand.UserId)).Description);

        RuleFor(cmd => cmd.CodeType)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(ResendVerificationCodeCommand.CodeType)).Description)
            .IsInEnum()
            .WithMessage(Errors.General.UnexpectedValue(nameof(ResendVerificationCodeCommand.CodeType)).Description);

    }
}