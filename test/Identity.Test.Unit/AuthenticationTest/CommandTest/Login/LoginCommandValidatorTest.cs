using FluentAssertions;
using FluentValidation.TestHelper;
using Identity.Application.Features.Authentication.Commands.Login;

namespace Identity.Test.Unit.AuthenticationTest.CommandTest.Login;

public class LoginCommandValidatorTest
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "",
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("The value cannot be empty: Email ");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Exceeds_Max_Length()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = new string('a', 256),
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Value 'Email' should not exceed 255.");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "",
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("The value cannot be empty: Password ");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Exceeds_Max_Length()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = new('a', 256),
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Value 'Password' should not exceed 255.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }




}