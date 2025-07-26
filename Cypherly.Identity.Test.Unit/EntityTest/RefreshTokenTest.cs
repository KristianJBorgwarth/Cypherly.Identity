using Cypherly.Identity.Domain.Entities;
using FluentAssertions;

namespace Cypherly.Authentication.Test.Unit.EntityTest;

public class RefreshTokenTest
{
    [Fact]
    public void Revoke_RevokesToken()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid());

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.Revoked.Should().NotBeNull();
        refreshToken.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_ThrowsException_If_Already_Revoked()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid());
        refreshToken.Revoke();

        // Act
        Action act = () => refreshToken.Revoke();

        // Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Refresh token is already revoked.");
    }

    [Fact]
    public void IsExpired_Returns_True_If_Expired()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(-1));
        // Act
        var result = refreshToken.IsExpired;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_Returns_False_If_Not_Expired()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid());
        // Act
        var result = refreshToken.IsExpired;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_Returns_True_If_Not_Revoked_And_Not_Expired()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = refreshToken.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_Returns_False_If_Revoked()
    {
        // Arrange
        var refreshToken = new RefreshToken(Guid.NewGuid(), Guid.NewGuid());
        refreshToken.Revoke();

        // Act
        var result = refreshToken.IsValid();

        // Assert
        result.Should().BeFalse();
    }
}