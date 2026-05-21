using FluentAssertions;
using FluentValidation.TestHelper;
using Identity.Application.Features.User.Commands.Create;

namespace Identity.Test.Unit.UserTest.CommandTest.CreateTest;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _sut = new();

    [Fact]
    public void Validate_Given_Valid_Command_Should_Return_Valid()
    {
        // Arrange
        var cmd = new CreateUserCommand()
        {
            Email = "valid@email.dk",
            Password = "validPassword97?",
            Username = "validUsername"

        };

        // Act
        var result = _sut.TestValidate(cmd);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Given_Empty_Email_Should_Return_Invalid()
    {
        // Arrange
        var cmd = new CreateUserCommand()
        {
            Email = "",
            Password = "validPassword97?",
            Username = "validUsername"
        };

        // Act
        var result = _sut.TestValidate(cmd);

        // Assert
        result.ShouldHaveValidationErrors();
        result.Errors[0].ErrorMessage.Should().Be($"The value cannot be empty: {nameof(CreateUserCommand.Email)} ");
    }

    [Fact]
    public void Validate_Given_Empty_Password_Should_Return_Invalid()
    {
        // Arrange
        var cmd = new CreateUserCommand()
        {
            Email = "valid@email.dk",
            Password = "",
            Username = "validUsername"
        };

        // Act
        var result = _sut.TestValidate(cmd);

        // Assert
        result.ShouldHaveValidationErrors();
        result.Errors[0].ErrorMessage.Should()
            .Be($"The value cannot be empty: {nameof(CreateUserCommand.Password)} ");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("")]
    [InlineData("ælkajdlækjasldkjalæsklæaskjdæaslkjd")]
    public void Validate_Given_Invalid_Username_Should_Return_Invalid(string username)
    {
        // Arrange
        var cmd = new CreateUserCommand()
        {
            Email = "valid@email.dk",
            Password = "validPassword97?",
            Username = username
        };

        // Act
        var result = _sut.TestValidate(cmd);


        // Assert
        result.ShouldHaveValidationErrors();
        result.IsValid.Should().BeFalse();
    }
}