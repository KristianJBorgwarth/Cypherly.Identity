using FluentAssertions;
using Identity.Domain.ValueObjects;

namespace Identity.Test.Unit.ValueObjectTest;

public class PasswordTests
{

    [Theory]
    [InlineData("ValidPassword123!")]
    [InlineData("AnotherValid1@")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoSpecialChar1")]
    [InlineData("NoNumbers!")]
    [InlineData("correct horse battery staple")]
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
    [InlineData("1234567")]
    public void GivenInvalidPassword_Should_ReturnFailureResult(string plainPassword)
    {
        // Act
        var result = Password.Create(plainPassword);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Description.Should().Be("Incorrect password: Must be at least 8 characters and at most 72 bytes.");
    }

    [Theory]
    [InlineData('a', 72, true)]
    [InlineData('a', 73, false)]
    [InlineData('é', 36, true)]
    [InlineData('é', 37, false)]
    public void Password_Should_Be_Limited_To_72_Utf8_Bytes(char character, int count, bool expectedValid)
    {
        // Act
        var result = Password.Create(new string(character, count));

        // Assert
        result.Success.Should().Be(expectedValid);
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