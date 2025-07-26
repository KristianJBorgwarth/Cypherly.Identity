using FakeItEasy;
using FluentAssertions;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.Authentication.Commands.RefreshTokens;
using Identity.Application.Features.Authentication.Token;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using Identity.Domain.Services.User;
using Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Unit.AuthenticationTest.CommandTest.RefreshTokens;

public class RefreshTokensCommandHandlerTest
{
    private readonly IUserRepository _fakeUserRepo;
    private readonly IUnitOfWork _fakeUnitOfWork;
    private readonly IJwtService _fakeJwtService;
    private readonly IAuthenticationService _fakeAuthService;
    private readonly ILogger<RefreshTokensCommandHandler> _fakeLogger;
    private readonly RefreshTokensCommandHandler _sut;

    public RefreshTokensCommandHandlerTest()
    {
        _fakeUserRepo = A.Fake<IUserRepository>();
        _fakeUnitOfWork = A.Fake<IUnitOfWork>();
        _fakeJwtService = A.Fake<IJwtService>();
        _fakeAuthService = A.Fake<IAuthenticationService>();
        _fakeLogger = A.Fake<ILogger<RefreshTokensCommandHandler>>();
        _sut = new RefreshTokensCommandHandler(_fakeUserRepo, _fakeUnitOfWork, _fakeJwtService, _fakeAuthService, _fakeLogger);
    }

    [Fact]
    public async Task Handle_Given_Invalid_User_Id_Should_Return_ResultFail()
    {
        // Arrange
        var command = new RefreshTokensCommand
        {
            RefreshToken = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
        };

        A.CallTo(() => _fakeUserRepo.GetByIdAsync(command.UserId))!.Returns<User>(null!);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Match(Errors.General.NotFound(command.UserId).Message);
    }

    [Fact]
    public async Task Handle_Given_Invalid_Refresh_Token_Should_Return_ResultFail()
    {
        // Arrange
        var command = new RefreshTokensCommand
        {
            RefreshToken = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
        };

        var user = new User();
        A.CallTo(() => _fakeUserRepo.GetByIdAsync(command.UserId))!.Returns(user);
        A.CallTo(() => _fakeAuthService.VerifyRefreshToken(user, command.DeviceId, command.RefreshToken))!.Returns(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Match("Invalid refresh token");
    }

    [Fact]
    public async Task Handle_Given_Valid_Refresh_Token_Should_Return_ResultOk()
    {
        // Arrange
        var command = new RefreshTokensCommand
        {
            RefreshToken = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
        };

        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("testk98?kKLll"), true);
        A.CallTo(() => _fakeUserRepo.GetByIdAsync(command.UserId))!.Returns(user);
        A.CallTo(() => _fakeAuthService.VerifyRefreshToken(user, command.DeviceId, command.RefreshToken))!.Returns(true);
        A.CallTo(() => _fakeAuthService.GenerateRefreshToken(user, command.DeviceId)).Returns(new RefreshToken(Guid.NewGuid(), Guid.NewGuid()));
        A.CallTo(() => _fakeJwtService.GenerateToken(user.Id, command.DeviceId)).Returns(Guid.NewGuid().ToString());

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_Given_Exception_Should_Return_ResultFail()
    {
        // Arrange
        var command = new RefreshTokensCommand
        {
            RefreshToken = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
        };

        var user = new User();
        A.CallTo(() => _fakeUserRepo.GetByIdAsync(command.UserId))!.Returns(user);
        A.CallTo(() => _fakeAuthService.VerifyRefreshToken(user, command.DeviceId, command.RefreshToken))!.Throws<Exception>();

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Match("Error occured while refreshing tokens");
    }
}