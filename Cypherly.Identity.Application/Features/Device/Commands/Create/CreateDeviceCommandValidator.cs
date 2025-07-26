using System.Text.RegularExpressions;
using Cypherly.Domain.Common;
using Cypherly.Identity.Domain.Common;
using FluentValidation;

namespace Cypherly.Identity.Application.Features.Device.Commands.Create;

public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.UserId)).Message);

        RuleFor(x => x.LoginNonceId)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.LoginNonceId)).Message);

        RuleFor(x => x.LoginNonce)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.LoginNonce)).Message);

        RuleFor(x => x.Base64DevicePublicKey)
            .NotNull()
            .WithMessage(Errors.General.ValueIsRequired(nameof(CreateDeviceCommand.Base64DevicePublicKey)).Message)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.Base64DevicePublicKey)).Message)
            .Must(x => x.Length <= 100).WithMessage(Errors.General.ValueTooLarge(nameof(CreateDeviceCommand.Base64DevicePublicKey), 100).Message);

        RuleFor(x => x.DeviceAppVersion)
            .NotNull()
            .WithMessage(Errors.General.ValueIsRequired(nameof(CreateDeviceCommand.DeviceAppVersion)).Message)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.DeviceAppVersion)).Message)
            .Must(x => x.Length <= 6)
            .WithMessage(Errors.General.ValueTooLarge(nameof(CreateDeviceCommand.DeviceAppVersion), 6).Message)
            .Must(BeValidDeviceAppVersion)
            .WithMessage(Errors.General.UnexpectedValue(nameof(CreateDeviceCommand.DeviceAppVersion)).Message);

        RuleFor(x => x.DeviceType)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.DeviceType)).Message)
            .IsInEnum()
            .WithMessage(Errors.General.UnexpectedValue(nameof(CreateDeviceCommand.DeviceType)).Message);

        RuleFor(x => x.DevicePlatform)
            .NotEmpty()
            .WithMessage(Errors.General.ValueIsEmpty(nameof(CreateDeviceCommand.DevicePlatform)).Message)
            .IsInEnum()
            .WithMessage(Errors.General.UnexpectedValue(nameof(CreateDeviceCommand.DevicePlatform)).Message);
    }

    private static bool BeValidDeviceAppVersion(string appVersion)
    {
        return Regex.IsMatch(appVersion, @"^\d+\.\d{1,3}$");
    }
}