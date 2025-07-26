using Cypherly.Authentication.Test.Integration.Setup;
using Cypherly.Domain.Common;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Application.Features.Authentication.Commands.RefreshTokens;
using Cypherly.Identity.Application.Features.Authentication.Token;
using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Domain.Common;
using Cypherly.Identity.Domain.Entities;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.Services.User;
using Cypherly.Identity.Domain.ValueObjects;
using Cypherly.Identity.Infrastructure.Persistence.Context;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cypherly.Authentication.Test.Integration.AuthenticationTest.CommandTest.RefreshTokens;

public class RefreshTokensCommandHandlerTest : IntegrationTestBase
{
    private readonly RefreshTokensCommandHandler _sut;
    public RefreshTokensCommandHandlerTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<RefreshTokensCommandHandler>>();
        _sut = new RefreshTokensCommandHandler(repo, unitOfWork, jwtService, authService, logger);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_Returns_ResultFail()
    {
        // Arrange
        var request = new RefreshTokensCommand()
        {
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
            RefreshToken = "refreshToken",
        };

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Match(Errors.General.NotFound(request.UserId).Message);
    }

    [Fact]
    public async Task Handle_WhenRefreshToken_Is_Invalid_Return_Result_Fail()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("Test098??kkkl"), true);
        var device = new Device(Guid.NewGuid(), "somekey", "1.0", DeviceType.Desktop, DevicePlatform.Android, user.Id);
        device.AddRefreshToken();
        user.AddDevice(device);

        Db.Add(user);
        await Db.SaveChangesAsync();

        var command = new RefreshTokensCommand()
        {
            UserId = user.Id,
            DeviceId = device.Id,
            RefreshToken = "invalidToken",
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Match(Errors.General.UnspecifiedError("Invalid refresh token").Message);
    }

    [Fact]
    public async Task Handle_WhenRefreshToken_Is_Valid_Return_Result_Ok()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("Test098??kkkl"), true);
        var device = new Device(Guid.NewGuid(), "somekey", "1.0", DeviceType.Desktop, DevicePlatform.Android, user.Id);
        var token = device.AddRefreshToken();
        user.AddDevice(device);

        Db.Add(user);
        await Db.SaveChangesAsync();

        var command = new RefreshTokensCommand()
        {
            UserId = user.Id,
            DeviceId = device.Id,
            RefreshToken = token.Token,
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.RefreshToken.Should().NotBeNull().And.NotBeEmpty();
        result.Value.Jwt.Should().NotBeNull().And.NotBeEmpty();
    }
}