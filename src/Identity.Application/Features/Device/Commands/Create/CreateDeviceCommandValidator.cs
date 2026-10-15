using System.Text.RegularExpressions;
using Cypherly.Domain.Common;
using Identity.Domain.Common;
using FluentValidation;

namespace Identity.Application.Features.Device.Commands.Create;

public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.UserId)).Description);

        RuleFor(x => x.LoginNonceId)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.LoginNonceId)).Description);

        RuleFor(x => x.LoginNonce)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.LoginNonce)).Description);

        RuleFor(x => x.Base64DevicePublicKey)
            .NotNull()
            .WithMessage(Errors.General.ValueIsRequired(nameof(CreateDeviceCommand.Base64DevicePublicKey)).Description)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.Base64DevicePublicKey)).Description)
            .Must(x => x.Length <= 100).WithMessage(Errors.General.ValueTooLarge(nameof(CreateDeviceCommand.Base64DevicePublicKey), 100).Description);

        RuleFor(x => x.DeviceAppVersion)
            .NotNull()
            .WithMessage(Errors.General.ValueIsRequired(nameof(CreateDeviceCommand.DeviceAppVersion)).Description)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.DeviceAppVersion)).Description)
            .Must(x => x.Length <= 6)
            .WithMessage(Errors.General.ValueTooLarge(nameof(CreateDeviceCommand.DeviceAppVersion), 6).Description)
            .Must(BeValidDeviceAppVersion)
            .WithMessage(Errors.General.UnexpectedValue(nameof(CreateDeviceCommand.DeviceAppVersion)).Description);

        RuleFor(x => x.DeviceType)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.DeviceType)).Description)
            .IsInEnum()
            .WithMessage(Errors.General.UnexpectedValue(nameof(CreateDeviceCommand.DeviceType)).Description);

        RuleFor(x => x.DevicePlatform)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.DevicePlatform)).Description)
            .IsInEnum()
            .WithMessage(Errors.General.UnexpectedValue(nameof(CreateDeviceCommand.DevicePlatform)).Description);
    }

    private static bool BeValidDeviceAppVersion(string appVersion)
    {
        return Regex.IsMatch(appVersion, @"^\d+\.\d{1,3}$");
    }
}