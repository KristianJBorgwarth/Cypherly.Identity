using FakeItEasy;
using FluentAssertions;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.User.Commands.Update.Verify;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Unit.UserTest.CommandTest.UpdateTest.VerifyTest
{
    public class VerifyUserCommandHandlerTests
    {
        private readonly IUserRepository _fakeUserRepository;
        private readonly IUnitOfWork _fakeUnitOfWork;
        private readonly VerifyUserCommandHandler _sut;

        public VerifyUserCommandHandlerTests()
        {
            var fakeLogger = A.Fake<ILogger<VerifyUserCommandHandler>>();
            _fakeUserRepository = A.Fake<IUserRepository>();
            _fakeUnitOfWork = A.Fake<IUnitOfWork>();

            _sut = new(fakeLogger, _fakeUserRepository, _fakeUnitOfWork);
        }

        [Fact]
        public async Task Handle_Should_Return_ResultOk_When_Verification_Is_Successful()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var verificationCode = "validCode";
            var email = Email.Create("test@mail.com").Value;
            var password = Password.Create("Password123!").Value;
            var user = new User(Guid.NewGuid(), email, password, isVerified: false);
            user.AddVerificationCode(UserVerificationCodeType.EmailVerification);  // Setting the verification code

            A.CallTo(() => _fakeUserRepository.GetByIdAsync(userId)).Returns(user);
            A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>.Ignored)).DoesNothing();

            var command = new VerifyUserCommand
            {
                UserId = userId,
                VerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.EmailVerification)!.Code.Value
            };

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            A.CallTo(() => _fakeUserRepository.UpdateAsync(user)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handle_Should_Return_ResultFail_When_User_Not_Found()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new VerifyUserCommand
            {
                UserId = userId,
                VerificationCode = "someCode"
            };

            A.CallTo(() => _fakeUserRepository.GetByIdAsync(userId))!.Returns<User>(null!);

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Error.Message.Should().Be(Errors.General.NotFound(userId).Message);
            A.CallTo(() => _fakeUserRepository.UpdateAsync(A<User>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handle_Should_Return_ResultFail_When_Verification_Fails()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = Email.Create("test@mail.com").Value!;
            var password = Password.Create("Password123!").Value!;
            var user = new User(Guid.NewGuid(), email, password, isVerified: false);
            user.AddVerificationCode(UserVerificationCodeType.EmailVerification);  // Setting the verification code

            A.CallTo(() => _fakeUserRepository.GetByIdAsync(userId)).Returns(user);
            A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>.Ignored)).DoesNothing();

            var command = new VerifyUserCommand
            {
                UserId = userId,
                VerificationCode = "invalidCode"
            };

            // Act
            var result = await _sut.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Error.Message.Should().Be("Invalid verification code");  // Adjust based on actual message
            A.CallTo(() => _fakeUserRepository.UpdateAsync(A<User>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }
    }
}
