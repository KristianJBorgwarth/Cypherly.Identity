using FluentValidation;

namespace Identity.Application.Features.Device.Commands.Delete;

public sealed class DeleteDeviceCommandValidator : AbstractValidator<DeleteDeviceCommand>
{
    public DeleteDeviceCommandValidator()
    {
        RuleFor(x => x.TargetDeviceId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();
    }
}
