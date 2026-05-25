using FluentAssertions;
using Identity.Application.Features.Authentication.Commands.VerifyNonce;

namespace Identity.Test.Unit.AuthenticationTest.CommandTest.VerifyNonce;

public class VerifyNonceCommandValidatorTest
{
    private readonly VerifyNonceCommandValidator _validator = new();

    [Fact]
    public void VerifyNonceCommandValidator_WhenUserIdIsEmpty_ShouldHaveError()
    {
        var command = new VerifyNonceCommand
        {
            UserId = Guid.Empty,
            NonceId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
            Nonce = "nonce"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(VerifyNonceCommand.UserId));
    }

    [Fact]
    public void VerifyNonceCommandValidator_WhenNonceIdIsEmpty_ShouldHaveError()
    {
        var command = new VerifyNonceCommand
        {
            UserId = Guid.NewGuid(),
            NonceId = Guid.Empty,
            DeviceId = Guid.NewGuid(),
            Nonce = "nonce"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(VerifyNonceCommand.NonceId));
    }

    [Fact]
    public void VerifyNonceCommandValidator_WhenDeviceIdIsEmpty_ShouldHaveError()
    {
        var command = new VerifyNonceCommand
        {
            UserId = Guid.NewGuid(),
            NonceId = Guid.NewGuid(),
            DeviceId = Guid.Empty,
            Nonce = "nonce"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(VerifyNonceCommand.DeviceId));
    }

    [Fact]
    public void VerifyNonceCommandValidator_WhenNonceIsEmpty_ShouldHaveError()
    {
        var command = new VerifyNonceCommand
        {
            UserId = Guid.NewGuid(),
            NonceId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
            Nonce = string.Empty
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(VerifyNonceCommand.Nonce));
    }
}