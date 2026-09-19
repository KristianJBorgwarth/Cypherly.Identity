using FakeItEasy;
using FluentAssertions;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.RateLimiting;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.Authentication.Commands.VerifyLogin;
using Identity.Domain.Aggregates;
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

    [Fact]
    public async Task Handle_Given_Valid_Command_Should_Return_LoginNonce_ResultOk()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("kjash)023+23?JK"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);

        var command = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.Login)!.Code.Value,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Nonce.Should().NotBeNullOrEmpty();
        A.CallTo(() => _fakeNonceCache.AddNonceAsync(A<LoginNonce>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Given_Repo_Returns_Null_Should_Return_ResultFail()
    {
        // Arrange
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns((User)null!);

        var command = new VerifyLoginCommand()
        {
            UserId = Guid.NewGuid(),
            LoginVerificationCode = "123456",
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

        var command = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = "invalid code",
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Contain("Invalid verification code");
        A.CallTo(() => _fakeNonceCache.AddNonceAsync(A<LoginNonce>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Given_RateLimit_Exceeded_Should_Return_ResultFail()
    {
        // Arrange
        A.CallTo(() => _fakeRateLimiter.IsAllowedAsync(A<string>._, A<int>._, A<TimeSpan>._, A<CancellationToken>._)).Returns(false);

        var command = new VerifyLoginCommand()
        {
            UserId = Guid.NewGuid(),
            LoginVerificationCode = "123456",
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Contain("Too many requests");
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

        var wrongCommand = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = "wrong-code",
        };

        // Act - exhaust the attempt cap with wrong guesses
        await _sut.Handle(wrongCommand, CancellationToken.None);
        await _sut.Handle(wrongCommand, CancellationToken.None);
        await _sut.Handle(wrongCommand, CancellationToken.None);

        var correctCommand = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = correctCode,
        };
        var result = await _sut.Handle(correctCommand, CancellationToken.None);

        // Assert - the code is burned even though the correct value is now supplied
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Contain("Invalid verification code");
    }
}
