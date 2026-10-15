using FakeItEasy;
using FluentAssertions;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.RateLimiting;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.User.Commands.Update.ResendVerificationCode;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Identity.Domain.Enums;
using Identity.Domain.Services.User;
using Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Unit.UserTest.CommandTest.UpdateTest.ResendAccountVerificationCommand;

public class ResendVerificationCodeCommandHandlerTest
{
    private readonly IUserRepository _fakeRepo;
    private readonly IUnitOfWork _fakeUnitOfWork;
    private readonly IVerificationCodeService _fakeVerificationCodeService;
    private readonly IRateLimiter _fakeRateLimiter;
    private readonly ILoginNonceCache _fakeLoginNonceCache;
    private readonly ResendVerificationCodeCommandHandler _sut;

    public ResendVerificationCodeCommandHandlerTest()
    {
        _fakeRepo = A.Fake<IUserRepository>();
        _fakeUnitOfWork = A.Fake<IUnitOfWork>();
        _fakeVerificationCodeService = A.Fake<IVerificationCodeService>();
        _fakeRateLimiter = A.Fake<IRateLimiter>();
        A.CallTo(() => _fakeRateLimiter.IsAllowedAsync(A<string>._, A<int>._, A<TimeSpan>._, A<CancellationToken>._)).Returns(true);
        _fakeLoginNonceCache = A.Fake<ILoginNonceCache>();
        var fakeLogger = A.Fake<ILogger<ResendVerificationCodeCommandHandler>>();
        _sut = new ResendVerificationCodeCommandHandler(_fakeRepo, _fakeUnitOfWork, _fakeVerificationCodeService, _fakeRateLimiter, _fakeLoginNonceCache, fakeLogger);
    }

    [Fact]
    public async Task Handle_Command_With_Invalid_Id_Should_Return_Result_Fail()
    {
        // Arrange
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = Guid.NewGuid(),
            CodeType = UserVerificationCodeType.EmailVerification,
        };
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._))!.Returns<User>(null);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Code.Should().Be("entity.not.found");
    }

    [Fact]
    public async Task Handle_Command_With_User_Verified_Should_Return_Result_Fail()
    {
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), true);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.EmailVerification,
        };
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Description.Should().Be("User is already verified");
    }

    [Fact]
    public async Task Handle_Command_With_Valid_Id_Should_Generate_New_Verification_Code_And_Return_Result_Ok()
    {

        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), false);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.EmailVerification,

        };
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(user, UserVerificationCodeType.EmailVerification)).DoesNothing();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(CancellationToken.None)).DoesNothing();

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Success.Should().BeTrue();
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(user, UserVerificationCodeType.EmailVerification)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(CancellationToken.None)).MustHaveHappenedOnceExactly();

    }

    [Fact]
    public async Task Handle_Given_RateLimit_Exceeded_Should_Return_TooManyRequests()
    {
        // Arrange
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = Guid.NewGuid(),
            CodeType = UserVerificationCodeType.EmailVerification,
        };
        A.CallTo(() => _fakeRateLimiter.IsAllowedAsync($"resend:EmailVerification:{cmd.UserId}", A<int>._, A<TimeSpan>._, A<CancellationToken>._)).Returns(false);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.TooManyRequests());
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_Login_With_Valid_Password_Nonce_Should_Generate_New_Code()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), true);
        var nonce = LoginNonce.Create(user.Id, LoginNonceStage.PasswordVerified);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.Login,
            NonceId = nonce.Id,
            Nonce = nonce.NonceValue,
        };
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);
        A.CallTo(() => _fakeLoginNonceCache.GetNonceAsync(nonce.Id, A<CancellationToken>._)).Returns(nonce);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(user, UserVerificationCodeType.Login)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Given_Login_Without_Nonce_Should_Return_Unauthorized()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), true);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.Login,
        };
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(A<User>._, A<UserVerificationCodeType>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeRateLimiter.IsAllowedAsync(A<string>._, A<int>._, A<TimeSpan>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_Login_RateLimit_Exceeded_With_Valid_Nonce_Should_Return_TooManyRequests()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), true);
        var nonce = LoginNonce.Create(user.Id, LoginNonceStage.PasswordVerified);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.Login,
            NonceId = nonce.Id,
            Nonce = nonce.NonceValue,
        };
        A.CallTo(() => _fakeLoginNonceCache.GetNonceAsync(nonce.Id, A<CancellationToken>._)).Returns(nonce);
        A.CallTo(() => _fakeRateLimiter.IsAllowedAsync($"resend:Login:{user.Id}", A<int>._, A<TimeSpan>._, A<CancellationToken>._)).Returns(false);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.TooManyRequests());
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(A<User>._, A<UserVerificationCodeType>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_Login_With_Expired_Nonce_Should_Return_Unauthorized()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), true);
        var nonce = LoginNonce.FromCache(Guid.NewGuid(), "nonce", user.Id, LoginNonceStage.PasswordVerified, DateTime.UtcNow.AddMinutes(-31), DateTime.UtcNow.AddMinutes(-1));
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.Login,
            NonceId = nonce.Id,
            Nonce = nonce.NonceValue,
        };
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);
        A.CallTo(() => _fakeLoginNonceCache.GetNonceAsync(nonce.Id, A<CancellationToken>._)).Returns(nonce);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(A<User>._, A<UserVerificationCodeType>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_Login_With_CodeVerified_Nonce_Should_Return_Unauthorized()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), true);
        var nonce = LoginNonce.Create(user.Id, LoginNonceStage.CodeVerified);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.Login,
            NonceId = nonce.Id,
            Nonce = nonce.NonceValue,
        };
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);
        A.CallTo(() => _fakeLoginNonceCache.GetNonceAsync(nonce.Id, A<CancellationToken>._)).Returns(nonce);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(A<User>._, A<UserVerificationCodeType>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_Login_With_Nonce_Of_Another_User_Should_Return_Unauthorized()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), true);
        var nonce = LoginNonce.Create(Guid.NewGuid(), LoginNonceStage.PasswordVerified);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.Login,
            NonceId = nonce.Id,
            Nonce = nonce.NonceValue,
        };
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);
        A.CallTo(() => _fakeLoginNonceCache.GetNonceAsync(nonce.Id, A<CancellationToken>._)).Returns(nonce);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(A<User>._, A<UserVerificationCodeType>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_PasswordReset_Should_Return_Fail()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), true);
        var cmd = new ResendVerificationCodeCommand
        {
            UserId = user.Id,
            CodeType = UserVerificationCodeType.PasswordReset,
        };
        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Code.Should().Be("unexpected.value");
        A.CallTo(() => _fakeVerificationCodeService.GenerateVerificationCode(A<User>._, A<UserVerificationCodeType>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }
}
