using FluentAssertions;
using Identity.Domain.ValueObjects;

namespace Identity.Test.Unit.ValueObjectTest;

public class PasswordTests
{

    [Theory]
    [InlineData("ValidPassword123!")]
    [InlineData("AnotherValid1@")]
    public void GivenValidPassword_Should_Hash_Correctly(string plainPassword)
    {
        // Act
        var result = Password.Create(plainPassword);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.HashedPassword.Should().NotBeNullOrEmpty();
        result.Value.HashedPassword.Should().NotBe(plainPassword); // Ensure it is hashed
    }

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoSpecialChar1")]
    [InlineData("NoNumbers!")]
    public void GivenInvalidPassword_Should_ReturnFailureResult(string plainPassword)
    {
        // Act
        var result = Password.Create(plainPassword);

        // Assert
        result.Success.Should().BeFalse();
        result.Error!.Description.Should().Be("Incorrect password: Must be between 8 and 36 characters, contain one uppercase letter, one lowercase letter, one special character, and no spaces.");
    }

    [Theory]
    [InlineData("ValidPassword123!", "ValidPassword123!")]
    [InlineData("AnotherValid1@", "AnotherValid1@")]
    public void Password_Verification_Should_Succeed_For_Correct_Password(string plainPassword, string verifyPassword)
    {
        // Act
        var result = Password.Create(plainPassword);
        var isVerified = result.Value.Verify(verifyPassword);

        // Assert
        result.Success.Should().BeTrue();
        isVerified.Should().BeTrue();
    }

    [Theory]
    [InlineData("ValidPassword123!", "WrongPassword123!")]
    [InlineData("AnotherValid1@", "IncorrectPassword1@")]
    public void Password_Verification_Should_Fail_For_Incorrect_Password(string plainPassword, string wrongPassword)
    {
        // Act
        var result = Password.Create(plainPassword);
        var isVerified = result.Value.Verify(wrongPassword);

        // Assert
        result.Success.Should().BeTrue();
        isVerified.Should().BeFalse();
    }
}