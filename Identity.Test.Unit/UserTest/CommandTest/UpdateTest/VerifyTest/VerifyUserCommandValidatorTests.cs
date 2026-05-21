using FluentValidation.TestHelper;
using Identity.Application.Features.User.Commands.Update.Verify;

namespace Identity.Test.Unit.UserTest.CommandTest.UpdateTest.VerifyTest
{
    public class VerifyUserCommandValidatorTests
    {
        private readonly VerifyUserCommandValidator _validator = new();

        [Fact]
        public void Should_Have_Error_When_UserId_Is_Empty()
        {
            // Arrange
            var command = new VerifyUserCommand
            {
                UserId = Guid.Empty, // Invalid
                VerificationCode = "validCode123"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(cmd => cmd.UserId)
                .WithErrorMessage($"The value cannot be empty: {nameof(VerifyUserCommand.UserId)} ");
        }

        [Fact]
        public void Should_Have_Error_When_VerificationCode_Is_Empty()
        {
            // Arrange
            var command = new VerifyUserCommand
            {
                UserId = Guid.NewGuid(),
                VerificationCode = string.Empty // Invalid
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(cmd => cmd.VerificationCode)
                .WithErrorMessage($"The value cannot be empty: {nameof(VerifyUserCommand.VerificationCode)} ");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Valid_Command_Is_Provided()
        {
            // Arrange
            var command = new VerifyUserCommand
            {
                UserId = Guid.NewGuid(),
                VerificationCode = "validCode123"
            };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(cmd => cmd.UserId);
            result.ShouldNotHaveValidationErrorFor(cmd => cmd.VerificationCode);
        }
    }
}
