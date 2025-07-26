using System.Text.Json;
using FluentAssertions;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.Authentication.Commands.VerifyLogin;
using Identity.Domain.Aggregates;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Persistence.Context;
using Identity.Test.Integration.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Integration.AuthenticationTest.CommandTest.VerifyLogin;

public class VerifyLoginCommandHandlerTest : IntegrationTestBase
{
    private readonly VerifyLoginCommandHandler _commandHandler;
    public VerifyLoginCommandHandlerTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<ILoginNonceCache>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<VerifyLoginCommandHandler>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        _commandHandler = new VerifyLoginCommandHandler(repo, cache, unitOfWork, logger);
    }

    [Fact]
    public async Task Handle_Given_Valid_Login_Verification_Code_Should_Return_Nonce()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("test??KL99"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        Db.User.Add(user);
        await Db.SaveChangesAsync();

        var cmd = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.Login)!.Code.Value,
        };

        // Act
        var result = await _commandHandler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var options = new JsonSerializerOptions()
        {
            Converters = { new LoginNonceJsonConverter() },
        };

        var nonce = await Cache.GetAsync<LoginNonce>(result.Value!.NonceId.ToString(), options, new CancellationToken());
        nonce.Should().NotBeNull();
        nonce!.UserId.Should().Be(user.Id);
        Db.User.AsNoTracking().Include(user => user.VerificationCodes).First().VerificationCodes.First().Code.IsUsed.Should().BeTrue();
        await Cache.RemoveAsync(result.Value!.NonceId.ToString(), new CancellationToken());
    }

    [Fact]
    public async Task Handle_Given_Invalid_Login_Verification_Code_Should_Return_Error()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("test??KL99"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        Db.User.Add(user);
        await Db.SaveChangesAsync();

        var cmd = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = "InvalidCode",
        };

        // Act
        var result = await _commandHandler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Invalid verification code");
    }

    [Fact]
    public async Task Handle_Given_Invalid_User_Id_Should_Return_Error()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("test??KL99"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        Db.User.Add(user);
        await Db.SaveChangesAsync();

        var cmd = new VerifyLoginCommand()
        {
            UserId = Guid.NewGuid(),
            LoginVerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.Login)!.Code.Value
        };

        // Act
        var result = await _commandHandler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Message.Should().Contain("Could not find entity with ID");
    }

}