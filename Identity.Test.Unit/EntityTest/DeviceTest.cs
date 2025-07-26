
using FluentAssertions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;

namespace Cypherly.Authentication.Test.Unit.EntityTest;

public class DeviceTest
{
    [Fact]
    public void AddRefreshToken_Should_AddRefreshToken()
    {
        // Arrange
        var device = new Device(Guid.NewGuid(), "publicKey", "1.0.0", DeviceType.Mobile,
            DevicePlatform.iOS, Guid.NewGuid());

        // Act
        device.AddRefreshToken();

        // Assert
        device.RefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public void GetActiveRefreshToken_Should_Return_Null_When_No_Token()
    {
        // Arrange
        var device = new Device(Guid.NewGuid(), "publicKey", "1.0.0", DeviceType.Mobile,
            DevicePlatform.iOS, Guid.NewGuid());

        // Act
        var result = device.GetActiveRefreshToken();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetActiveRefreshToken_Should_Return_Token_When_Token_Exists()
    {
        // Arrange
        var device = new Device(Guid.NewGuid(), "publicKey", "1.0.0", DeviceType.Mobile,
            DevicePlatform.iOS, Guid.NewGuid());

        device.AddRefreshToken();

        // Act
        var result = device.GetActiveRefreshToken();

        // Assert
        result.Should().NotBeNull();
    }
}