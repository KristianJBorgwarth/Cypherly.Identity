using FakeItEasy;
using FluentAssertions;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Domain.Aggregates;
using Identity.Domain.Services.User;
using Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Unit.AuthenticationTest.CommandTest.Login;

public class LoginCommandHandlerTest
{
    private readonly IUserRepository _fakeUserRepository;
    private readonly IUnitOfWork _fakeUnitOfWork;
    private readonly IAuthenticationService _fakeAuthService;
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTest()
    {
        _fakeUserRepository = A.Fake<IUserRepository>();
        _fakeAuthService = A.Fake<IAuthenticationService>();
        _fakeUnitOfWork = A.Fake<IUnitOfWork>();

        _sut = new LoginCommandHandler(_fakeUserRepository, _fakeAuthService, _fakeUnitOfWork, A.Fake<ILogger<LoginCommandHandler>>());
    }

    [Fact]
    public async void Handle_Given_Invalid_Email_Should_Return_InvalidCredentials()
    {
        // Arrange
        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).Returns((User)null);

        var cmd = new LoginCommand()
        {
            Email = "Test@mail.dk",
            Password = "TestPassword?123",
        };

        // Act
        var result = await _sut.Handle(cmd, default);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Contain("Invalid Credentials");
        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeAuthService.GenerateLoginVerificationCode(A<User>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(default)).MustNotHaveHappened();
    }

    [Fact]
    public async void Handle_Given_Invalid_Password_Should_Return_InvalidCredentials()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kj9203KKJHSD?23"), true);

        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).Returns(user);

        var cmd = new LoginCommand()
        {
            Email = "Test@mail.dk",
            Password = "THIS PASSWORD WILL BE INVALID GG",
        };

        // Act
        var result = await _sut.Handle(cmd, default);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Contain("Invalid Credentials");
        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeAuthService.GenerateLoginVerificationCode(A<User>._)).MustNotHaveHappened();

        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(default)).MustNotHaveHappened();
    }

    [Fact]
    public async void Handle_Given_Unverified_User_Should_Return_LoginDto_With_IsVerified_False()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kj9203KKJHSD?23"), false);

        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).Returns(user);

        var cmd = new LoginCommand()
        {
            Email = "Test@mail.dk",
            Password = "kj9203KKJHSD?23",
        };

        // Act
        var result = await _sut.Handle(cmd, default);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsVerified.Should().BeFalse();
        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeAuthService.GenerateLoginVerificationCode(A<User>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(default)).MustNotHaveHappened();
    }

    [Fact]
    public async void Handle_Given_Valid_Credentials_Should_Return_Result_Ok_With_LoginDto()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kj9203KKJHSD?23"), true);

        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).Returns(user);

        var cmd = new LoginCommand()
        {
            Email = "Test@mail.dk",
            Password = "kj9203KKJHSD?23",
        };

        // Act
        var result = await _sut.Handle(cmd, default);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsVerified.Should().BeTrue();
        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeAuthService.GenerateLoginVerificationCode(A<User>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(default)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async void Handle_Given_Exception_Should_Return_UnspecifiedError()
    {
        // Arrange
        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).Throws<Exception>();

        var cmd = new LoginCommand()
        {
            Email = "Test@mail.dk",
            Password = "kj9203KKJHSD?23",
        };

        // Act
        var result = await _sut.Handle(cmd, default);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Contain("An exception occured while attempting to login");
        A.CallTo(() => _fakeUserRepository.GetByEmailAsync(A<string>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeAuthService.GenerateLoginVerificationCode(A<User>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(default)).MustNotHaveHappened();
    }


}