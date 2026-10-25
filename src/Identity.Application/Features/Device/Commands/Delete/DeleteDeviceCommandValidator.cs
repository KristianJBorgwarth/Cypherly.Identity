using FluentValidation;

namespace Identity.Application.Features.Device.Commands.Delete;

public sealed class DeleteDeviceCommandValidator : AbstractValidator<DeleteDeviceCommand>
{
    public DeleteDeviceCommandValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("DeviceId is required")
            .MaximumLength(100).WithMessage("DeviceId must not exceed 100 characters");
    }
}
