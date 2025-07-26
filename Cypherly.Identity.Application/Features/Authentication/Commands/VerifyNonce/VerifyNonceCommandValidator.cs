using Cypherly.Domain.Common;
using Cypherly.Identity.Domain.Common;
using FluentValidation;

namespace Cypherly.Identity.Application.Features.Authentication.Commands.VerifyNonce;

public class VerifyNonceCommandValidator : AbstractValidator<VerifyNonceCommand>
{
    public VerifyNonceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(VerifyNonceCommand.UserId)).Message);
        RuleFor(x => x.NonceId)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(VerifyNonceCommand.NonceId)).Message);
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage(Errors.General.ValueIsEmpty(nameof(VerifyNonceCommand.DeviceId)).Message);
        RuleFor(x => x.Nonce)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(VerifyNonceCommand.Nonce)).Message);
    }
}