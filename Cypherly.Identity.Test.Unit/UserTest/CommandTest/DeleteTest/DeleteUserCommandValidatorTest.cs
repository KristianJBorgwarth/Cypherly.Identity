using Cypherly.Identity.Application.Features.User.Commands.Delete;
using FluentAssertions;

namespace Cypherly.Authentication.Test.Unit.UserTest.CommandTest.DeleteTest;

public class DeleteUserCommandValidatorTest
{
    private readonly DeleteUserCommandValidator _sut = new();

    [Fact]
    public void GivenDeleteUserCommandValidator_WhenIdIsEmpty_ThenShouldHaveValidationError()
    {
        // Arrange
        var command = new DeleteUserCommand { Id = Guid.Empty };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().ErrorMessage.Should().Contain("The value cannot be empty");
    }

    [Fact]
    public void GivenDeleteUserCommandValidator_WhenIdIsNotEmpty_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var command = new DeleteUserCommand { Id = Guid.NewGuid() };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}