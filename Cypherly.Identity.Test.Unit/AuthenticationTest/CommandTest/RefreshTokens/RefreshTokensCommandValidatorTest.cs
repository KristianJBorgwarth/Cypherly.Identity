using Cypherly.Identity.Application.Features.Authentication.Commands.RefreshTokens;
using FluentAssertions;

namespace Cypherly.Authentication.Test.Unit.AuthenticationTest.CommandTest.RefreshTokens;

public class RefreshTokensCommandValidatorTest
{
    private readonly RefreshTokensCommandValidator _sut = new();

    [Fact]
    public void RefreshTokensCommandValidator_WhenRefreshTokenIsEmpty_ShouldHaveError()
    {
        // Arrange
        var command = new RefreshTokensCommand
        {
            RefreshToken = string.Empty,
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(RefreshTokensCommand.RefreshToken));
    }

    [Fact]
    public void RefreshTokensCommandValidator_WhenUserIdIsEmpty_ShouldHaveError()
    {
        // Arrange
        var command = new RefreshTokensCommand
        {
            RefreshToken = Guid.NewGuid().ToString(),
            UserId = Guid.Empty,
            DeviceId = Guid.NewGuid(),
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(RefreshTokensCommand.UserId));
    }

    [Fact]
    public void RefreshTokensCommandValidator_WhenDeviceIdIsEmpty_ShouldHaveError()
    {
        // Arrange
        var command = new RefreshTokensCommand
        {
            RefreshToken = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            DeviceId = Guid.Empty,
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(RefreshTokensCommand.DeviceId));
    }
}