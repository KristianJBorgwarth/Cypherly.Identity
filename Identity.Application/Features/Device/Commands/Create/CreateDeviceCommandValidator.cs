using System.Text.RegularExpressions;
using FluentValidation;

namespace Identity.Application.Features.Device.Commands.Create;

public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateDeviceCommand.UserId)} ");

        RuleFor(x => x.LoginNonceId)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateDeviceCommand.LoginNonceId)} ");

        RuleFor(x => x.LoginNonce)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateDeviceCommand.LoginNonce)} ");

        RuleFor(x => x.Base64DevicePublicKey)
            .NotNull().WithMessage($"Value '{nameof(CreateDeviceCommand.Base64DevicePublicKey)}' is required.")
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateDeviceCommand.Base64DevicePublicKey)} ")
            .Must(x => x.Length <= 100).WithMessage($"Value '{nameof(CreateDeviceCommand.Base64DevicePublicKey)}' should not exceed 100.");

        RuleFor(x => x.DeviceAppVersion)
            .NotNull().WithMessage($"Value '{nameof(CreateDeviceCommand.DeviceAppVersion)}' is required.")
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateDeviceCommand.DeviceAppVersion)} ")
            .Must(x => x.Length <= 6).WithMessage($"Value '{nameof(CreateDeviceCommand.DeviceAppVersion)}' should not exceed 6.")
            .Must(BeValidDeviceAppVersion).WithMessage($"Value '{nameof(CreateDeviceCommand.DeviceAppVersion)}' is not valid in this context");

        RuleFor(x => x.DeviceType)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateDeviceCommand.DeviceType)} ")
            .IsInEnum().WithMessage($"Value '{nameof(CreateDeviceCommand.DeviceType)}' is not valid in this context");

        RuleFor(x => x.DevicePlatform)
            .NotEmpty().WithMessage($"The value cannot be empty: {nameof(CreateDeviceCommand.DevicePlatform)} ")
            .IsInEnum().WithMessage($"Value '{nameof(CreateDeviceCommand.DevicePlatform)}' is not valid in this context");
    }

    private static bool BeValidDeviceAppVersion(string appVersion)
    {
        return Regex.IsMatch(appVersion, @"^\d+\.\d{1,3}$");
    }
}
