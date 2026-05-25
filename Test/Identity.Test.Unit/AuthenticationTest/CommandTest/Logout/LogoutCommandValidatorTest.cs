using FluentAssertions;
using Identity.Application.Features.Authentication.Commands.Logout;

namespace Identity.Test.Unit.AuthenticationTest.CommandTest.Logout;

public class LogoutCommandValidatorTest
{
    private readonly LogoutCommandValidator _sut = new LogoutCommandValidator();

    [Fact]
    public void GivenValidRequest_WhenValidating_ThenShouldNotHaveValidationError()
    {
        // Arrange
        var request = new LogoutCommand
        {
            Id = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GivenInvalidRequest_WhenValidating_ThenShouldHaveValidationError()
    {
        // Arrange
        var request = new LogoutCommand
        {
            Id = Guid.Empty,
            DeviceId = Guid.Empty,
        };

        // Act
        var result = _sut.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}