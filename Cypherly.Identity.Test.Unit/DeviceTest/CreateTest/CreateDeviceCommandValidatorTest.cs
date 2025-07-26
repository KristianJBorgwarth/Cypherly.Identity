using Cypherly.Identity.Application.Features.Device.Commands.Create;
using Cypherly.Identity.Domain.Enums;
using FluentAssertions;

namespace Cypherly.Authentication.Test.Unit.DeviceTest.CreateTest;

public class CreateDeviceCommandValidatorTest
{
    private readonly CreateDeviceCommandValidator _validator = new();

    [Fact]
    public void GivenValidCommand_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var command = new CreateDeviceCommand
        {
            UserId = Guid.NewGuid(),
            LoginNonceId = Guid.NewGuid(),
            LoginNonce = "loginNonce",
            Base64DevicePublicKey = "base64DevicePublicKey",
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Desktop,
            DevicePlatform = DevicePlatform.Windows,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_Missing_UserId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDeviceCommand
        {
            UserId = Guid.Empty,
            LoginNonceId = Guid.NewGuid(),
            LoginNonce = "loginNonce",
            Base64DevicePublicKey = "base64DevicePublicKey",
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Desktop,
            DevicePlatform = DevicePlatform.Windows,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateDeviceCommand.UserId));
    }

    [Fact]
    public void Given_Missing_LoginNonceId_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDeviceCommand
        {
            UserId = Guid.NewGuid(),
            LoginNonceId = Guid.Empty,
            LoginNonce = "loginNonce",
            Base64DevicePublicKey = "base64DevicePublicKey",
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Desktop,
            DevicePlatform = DevicePlatform.Windows,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateDeviceCommand.LoginNonceId));
    }

    [Fact]
    public void Given_Missing_LoginNonce_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDeviceCommand
        {
            UserId = Guid.NewGuid(),
            LoginNonceId = Guid.NewGuid(),
            LoginNonce = string.Empty,
            Base64DevicePublicKey = "base64DevicePublicKey",
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Desktop,
            DevicePlatform = DevicePlatform.Windows,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateDeviceCommand.LoginNonce));
    }

    [Fact]
    public void Given_Missing_Base64DevicePublicKey_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDeviceCommand
        {
            UserId = Guid.NewGuid(),
            LoginNonceId = Guid.NewGuid(),
            LoginNonce = "loginNonce",
            Base64DevicePublicKey = string.Empty,
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Desktop,
            DevicePlatform = DevicePlatform.Windows,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateDeviceCommand.Base64DevicePublicKey));
    }

    [Fact]
    public void Given_Invalid_Base64DevicePublicKey_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDeviceCommand
        {
            UserId = Guid.NewGuid(),
            LoginNonceId = Guid.NewGuid(),
            LoginNonce = "loginNonce",
            Base64DevicePublicKey = string.Empty.PadLeft(101, 'a'),
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Desktop,
            DevicePlatform = DevicePlatform.Windows,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateDeviceCommand.Base64DevicePublicKey));
    }

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("")]
    [InlineData("jksjdf")]
    [InlineData("1.0k")]
    public void Given_Invalid_DeviceAppVersion_WhenValidating_ThenShouldHaveValidationError(string deviceAppVersion)
    {
        // Arrange
        var command = new CreateDeviceCommand
        {
            UserId = Guid.NewGuid(),
            LoginNonceId = Guid.NewGuid(),
            LoginNonce = "loginNonce",
            Base64DevicePublicKey = "base64DevicePublicKey",
            DeviceAppVersion = deviceAppVersion,
            DeviceType = DeviceType.Desktop,
            DevicePlatform = DevicePlatform.Windows,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateDeviceCommand.DeviceAppVersion));
    }

    [Fact]
    public void Given_Invalid_DeviceType_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDeviceCommand
        {
            UserId = Guid.NewGuid(),
            LoginNonceId = Guid.NewGuid(),
            LoginNonce = "loginNonce",
            Base64DevicePublicKey = "base64DevicePublicKey",
            DeviceAppVersion = "1.0",
            DeviceType = (DeviceType)3,
            DevicePlatform = DevicePlatform.Windows,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateDeviceCommand.DeviceType));
    }

    [Fact]
    public void Given_Invalid_DevicePlatform_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateDeviceCommand
        {
            UserId = Guid.NewGuid(),
            LoginNonceId = Guid.NewGuid(),
            LoginNonce = "loginNonce",
            Base64DevicePublicKey = "base64DevicePublicKey",
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Desktop,
            DevicePlatform = (DevicePlatform)100,
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateDeviceCommand.DevicePlatform));
    }
}