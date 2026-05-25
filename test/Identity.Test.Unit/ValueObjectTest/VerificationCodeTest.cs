using FluentAssertions;
using Identity.Domain.ValueObjects;

namespace Identity.Test.Unit.ValueObjectTest;

public class VerificationCodeTest
{
    [Fact]
    public void Create_ShouldGenerateValidCode_WithSpecifiedExpirationDate()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddMinutes(30);

        // Act
        var verificationCode = VerificationCode.Create(expirationDate);

        // Assert
        verificationCode.Should().NotBeNull();
        verificationCode.Value.Should().NotBeNullOrEmpty();
        verificationCode.Value.Length.Should().Be(6); // Verification code should be 6 digits
        verificationCode.ExpirationDate.Should().Be(expirationDate);
        verificationCode.IsUsed.Should().BeFalse();
    }

    [Fact]
    public void Verify_ShouldReturnSuccess_WhenCodeIsValid()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddMinutes(30);
        var verificationCode = VerificationCode.Create(expirationDate);
        var codeToVerify = verificationCode.Value;

        // Act
        var result = verificationCode.Verify(codeToVerify);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldFail_WhenCodeIsAlreadyUsed()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddMinutes(30);
        var verificationCode = VerificationCode.Create(expirationDate);
        var codeToVerify = verificationCode.Value;

        // Mark the code as used
        verificationCode.Use();

        // Act
        var result = verificationCode.Verify(codeToVerify);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be("Invalid verification code");
    }

    [Fact]
    public void Verify_ShouldFail_WhenCodeIsExpired()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddMinutes(-1); // Already expired
        var verificationCode = VerificationCode.Create(expirationDate);
        var codeToVerify = verificationCode.Value;

        // Act
        var result = verificationCode.Verify(codeToVerify);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be("Verification code has expired");
    }

    [Fact]
    public void Verify_ShouldFail_WhenCodeIsInvalid()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddMinutes(30);
        var verificationCode = VerificationCode.Create(expirationDate);
        var invalidCode = "123456"; // Some random invalid code

        // Act
        var result = verificationCode.Verify(invalidCode);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be("Invalid verification code");
    }

    [Fact]
    public void Use_ShouldMarkCodeAsUsed()
    {
        // Arrange
        var expirationDate = DateTime.UtcNow.AddMinutes(30);
        var verificationCode = VerificationCode.Create(expirationDate);

        // Act
        verificationCode.Use();

        // Assert
        verificationCode.IsUsed.Should().BeTrue();
    }
}