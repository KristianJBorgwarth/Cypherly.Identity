using Identity.Application.Caching;
using FluentAssertions;

namespace Cypherly.Authentication.Test.Unit.CachingTest;

public class NonceTest
{
    [Fact]
    public void CreateNonce_Should_Return_Nonce()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();

        // Act
        var nonce = Nonce.Create(userId, deviceId);

        // Assert
        nonce.Should().NotBeNull();
        nonce.UserId.Should().Be(userId);
        nonce.DeviceId.Should().Be(deviceId);
        nonce.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, new(0, 0, 1, 0, 0));
        nonce.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), new(0, 0, 1, 0, 0));
        nonce.Exipred.Should().BeFalse();
    }

    [Fact]
    public void FromCache_Should_Return_Nonce()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonceValue = "nonceValue";
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var nonce = Nonce.FromCache(id, nonceValue, userId, deviceId, createdAt, expiresAt);

        // Assert
        nonce.Should().NotBeNull();
        nonce.Id.Should().Be(id);
        nonce.NonceValue.Should().Be(nonceValue);
        nonce.UserId.Should().Be(userId);
        nonce.DeviceId.Should().Be(deviceId);
        nonce.CreatedAt.Should().Be(createdAt);
        nonce.ExpiresAt.Should().Be(expiresAt);
        nonce.Exipred.Should().BeFalse();
    }

    [Fact]
    public void FromCache_Should_Throw_ArgumentException_When_NonceValue_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonceValue = string.Empty;
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var expiresAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        Action act = () => Nonce.FromCache(id, nonceValue, userId, deviceId, createdAt, expiresAt);

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("Value cannot be null or whitespace. (Parameter 'nonceValue')");
    }
}