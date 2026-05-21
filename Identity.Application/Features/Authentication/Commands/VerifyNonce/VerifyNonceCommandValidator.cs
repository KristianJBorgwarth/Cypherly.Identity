using FluentValidation;

namespace Identity.Application.Features.Authentication.Commands.VerifyNonce;

public class VerifyNonceCommandValidator : AbstractValidator<VerifyNonceCommand>
{
    public VerifyNonceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(VerifyNonceCommand.UserId)} ");
        RuleFor(x => x.NonceId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(VerifyNonceCommand.NonceId)} ");
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(VerifyNonceCommand.DeviceId)} ");
        RuleFor(x => x.Nonce)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(VerifyNonceCommand.Nonce)} ");
    }
}
