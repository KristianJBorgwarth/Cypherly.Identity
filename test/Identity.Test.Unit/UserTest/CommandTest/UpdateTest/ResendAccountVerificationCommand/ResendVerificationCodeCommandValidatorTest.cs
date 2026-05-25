using FluentAssertions;
using Identity.Application.Features.User.Commands.Update.ResendVerificationCode;
using Identity.Domain.Enums;

namespace Identity.Test.Unit.UserTest.CommandTest.UpdateTest.ResendAccountVerificationCommand
{
    public class ResendVerificationCodeCommandValidatorTest
    {
        private readonly ResendVerificationCodeCommandValidator _sut = new();

        [Fact]
        public void Validate_ShouldHaveError_WhenUserIdIsEmpty()
        {
            // Arrange
            var command = new ResendVerificationCodeCommand
            {
                UserId = Guid.Empty,
                CodeType = UserVerificationCodeType.Login,
            };

            // Act
            var result = _sut.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.UserId));
        }

        [Fact]
        public void Validate_ShouldHaveNoErrors_WhenUserIdIsValid()
        {
            // Arrange
            var command = new ResendVerificationCodeCommand
            {
                UserId = Guid.NewGuid(),
                CodeType = UserVerificationCodeType.Login,
            };

            // Act
            var result = _sut.Validate(command);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Validate_ShouldHaveError_WhenCodeTypeIsEmpty()
        {
            // Arrange
            var command = new ResendVerificationCodeCommand
            {
                UserId = Guid.NewGuid(),
                CodeType = (UserVerificationCodeType)100,
            };

            // Act
            var result = _sut.Validate(command);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.CodeType));
        }
    }
}