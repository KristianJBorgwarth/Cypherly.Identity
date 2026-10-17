using FakeItEasy;
using FluentAssertions;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.RateLimiting;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.Authentication.Commands.VerifyLogin;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Unit.AuthenticationTest.CommandTest.VerifyLogin;

public class VerifyLoginCommandHandlerTest
{
    private readonly IUserRepository _fakeRepo;
    private readonly ILoginNonceCache _fakeNonceCache;
    private readonly IUnitOfWork _fakeUnitOfWork;
    private readonly IRateLimiter _fakeRateLimiter;
    private readonly VerifyLoginCommandHandler _sut;

    public VerifyLoginCommandHandlerTest()
    {
        _fakeRepo = A.Fake<IUserRepository>();
        _fakeNonceCache = A.Fake<ILoginNonceCache>();
        _fakeUnitOfWork = A.Fake<IUnitOfWork>();
        _fakeRateLimiter = A.Fake<IRateLimiter>();
        A.CallTo(() => _fakeRateLimiter.IsAllowedAsync(A<string>._, A<int>._, A<TimeSpan>._, A<CancellationToken>._)).Returns(true);
        var fakeLogger = A.Fake<ILogger<VerifyLoginCommandHandler>>();
        _sut = new VerifyLoginCommandHandler(_fakeRepo, _fakeNonceCache, _fakeUnitOfWork, _fakeRateLimiter, fakeLogger);
    }

    private LoginNonce ArrangePasswordNonce(Guid userId)
    {
        var nonce = LoginNonce.Create(userId, LoginNonceStage.PasswordVerified);
        A.CallTo(() => _fakeNonceCache.GetNonceAsync(nonce.Id, A<CancellationToken>._)).Returns(nonce);
        return nonce;
    }

    [Fact]
    public async Task Handle_Given_Valid_Command_Should_Return_LoginNonce_ResultOk()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjash)023+23?JK"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);
        var passwordNonce = ArrangePasswordNonce(user.Id);

        var command = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.Login)!.Code.Value,
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Nonce.Should().NotBeNullOrEmpty().And.NotBe(passwordNonce.NonceValue);
        result.Value.NonceId.Should().NotBe(passwordNonce.Id);
        A.CallTo(() => _fakeNonceCache.DeteleNonceAsync(passwordNonce.Id, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeNonceCache.AddNonceAsync(A<LoginNonce>.That.Matches(n => n.Stage == LoginNonceStage.CodeVerified && n.UserId == user.Id), A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Given_Repo_Returns_Null_Should_Return_ResultFail()
    {
        // Arrange
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns((User)null!);
        var userId = Guid.NewGuid();
        var passwordNonce = ArrangePasswordNonce(userId);

        var command = new VerifyLoginCommand()
        {
            UserId = userId,
            LoginVerificationCode = "123456",
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        A.CallTo(() => _fakeNonceCache.AddNonceAsync(A<LoginNonce>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_Invalid_VerificationCode_Should_Return_ResultFail()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjash)023+23?JK"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);
        var passwordNonce = ArrangePasswordNonce(user.Id);

        var command = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = "invalid code",
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Description.Should().Contain("Invalid verification code");
        A.CallTo(() => _fakeNonceCache.AddNonceAsync(A<LoginNonce>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeNonceCache.DeteleNonceAsync(A<Guid>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Given_RateLimit_Exceeded_Should_Return_ResultFail()
    {
        // Arrange
        A.CallTo(() => _fakeRateLimiter.IsAllowedAsync(A<string>._, A<int>._, A<TimeSpan>._, A<CancellationToken>._)).Returns(false);
        var userId = Guid.NewGuid();
        var passwordNonce = ArrangePasswordNonce(userId);

        var command = new VerifyLoginCommand()
        {
            UserId = userId,
            LoginVerificationCode = "123456",
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Description.Should().Contain("Too many requests");
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_Three_Failed_Attempts_Should_Burn_Code()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjash)023+23?JK"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);
        var correctCode = user.GetActiveVerificationCode(UserVerificationCodeType.Login)!.Code.Value;

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);
        var passwordNonce = ArrangePasswordNonce(user.Id);

        var wrongCommand = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = "wrong-code",
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act - exhaust the attempt cap with wrong guesses
        await _sut.Handle(wrongCommand, CancellationToken.None);
        await _sut.Handle(wrongCommand, CancellationToken.None);
        await _sut.Handle(wrongCommand, CancellationToken.None);

        var correctCommand = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = correctCode,
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };
        var result = await _sut.Handle(correctCommand, CancellationToken.None);

        // Assert - the code is burned even though the correct value is now supplied
        result.Success.Should().BeFalse();
        result.Error.Description.Should().Contain("Invalid verification code");
    }

    [Fact]
    public async Task Handle_Given_Missing_Nonce_Should_Return_Unauthorized_Without_Checking_Code()
    {
        // Arrange
        var command = new VerifyLoginCommand()
        {
            UserId = Guid.NewGuid(),
            LoginVerificationCode = "123456",
            NonceId = Guid.NewGuid(),
            Nonce = "nonce",
        };
        A.CallTo(() => _fakeNonceCache.GetNonceAsync(command.NonceId, A<CancellationToken>._)).Returns((LoginNonce?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeRateLimiter.IsAllowedAsync(A<string>._, A<int>._, A<TimeSpan>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_CodeVerified_Nonce_Should_Return_Unauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var nonce = LoginNonce.Create(userId, LoginNonceStage.CodeVerified);
        A.CallTo(() => _fakeNonceCache.GetNonceAsync(nonce.Id, A<CancellationToken>._)).Returns(nonce);

        var command = new VerifyLoginCommand()
        {
            UserId = userId,
            LoginVerificationCode = "123456",
            NonceId = nonce.Id,
            Nonce = nonce.NonceValue,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_Nonce_Of_Another_User_Should_Return_Unauthorized()
    {
        // Arrange
        var passwordNonce = ArrangePasswordNonce(Guid.NewGuid());

        var command = new VerifyLoginCommand()
        {
            UserId = Guid.NewGuid(),
            LoginVerificationCode = "123456",
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_Wrong_Nonce_Value_Should_Return_Unauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordNonce = ArrangePasswordNonce(userId);

        var command = new VerifyLoginCommand()
        {
            UserId = userId,
            LoginVerificationCode = "123456",
            NonceId = passwordNonce.Id,
            Nonce = "not-the-nonce",
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustNotHaveHappened();
    }
}
