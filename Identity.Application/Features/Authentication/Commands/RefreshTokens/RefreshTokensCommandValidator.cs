using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.RefreshTokens;

public class RefreshTokensCommandValidator : AbstractValidator<RefreshTokensCommand>
{
    public RefreshTokensCommandValidator()
    {
        RuleFor(p => p.RefreshToken)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(RefreshTokensCommand.RefreshToken)} ");

        RuleFor(p => p.UserId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(RefreshTokensCommand.UserId)} ");

        RuleFor(p => p.DeviceId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(RefreshTokensCommand.DeviceId)} ");
    }
}
