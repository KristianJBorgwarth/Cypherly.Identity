using FluentAssertions;
using Identity.Application.Features.User.Commands.Update.Verify;

namespace Identity.Test.Unit.AuthenticationTest.CommandTest.VerifyLogin;

public class VerifyLoginCommandValidatorTest
{
    private readonly VerifyUserCommandValidator _sut = new();

    [Fact]
    public void GivenValidCommand_WhenValidating_ThenValidationPasses()
    {
        var command = new VerifyUserCommand()
        {
            UserId = Guid.NewGuid(),
            VerificationCode = "123456",
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Given_Empty_UserId_WhenValidating_ThenValidationFails()
    {
        var command = new VerifyUserCommand()
        {
            UserId = Guid.Empty,
            VerificationCode = "123456",
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_Empty_VerificationCode_WhenValidating_ThenValidationFails()
    {
        var command = new VerifyUserCommand()
        {
            UserId = Guid.NewGuid(),
            VerificationCode = string.Empty,
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Given_Null_VerificationCode_WhenValidating_ThenValidationFails()
    {
        var command = new VerifyUserCommand()
        {
            UserId = Guid.NewGuid(),
            VerificationCode = null,
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}