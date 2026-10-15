using System.Text.Json;
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
using Identity.Infrastructure.Persistence.Context;
using Identity.Test.Integration.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Integration.AuthenticationTest.CommandTest.VerifyLogin;

public class VerifyLoginCommandHandlerTest : IntegrationTestBase
{
    private readonly VerifyLoginCommandHandler _commandHandler;
    private readonly ILoginNonceCache _loginNonceCache;
    public VerifyLoginCommandHandlerTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<ILoginNonceCache>();
        _loginNonceCache = cache;
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<VerifyLoginCommandHandler>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var rateLimiter = scope.ServiceProvider.GetRequiredService<IRateLimiter>();
        _commandHandler = new VerifyLoginCommandHandler(repo, cache, unitOfWork, rateLimiter, logger);
    }

    private async Task<LoginNonce> AddPasswordNonce(Guid userId)
    {
        var nonce = LoginNonce.Create(userId, LoginNonceStage.PasswordVerified);
        await _loginNonceCache.AddNonceAsync(nonce, CancellationToken.None);
        return nonce;
    }

    [Fact]
    public async Task Handle_Given_Valid_Login_Verification_Code_Should_Return_Nonce()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("test??KL99"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        Db.User.Add(user);
        await Db.SaveChangesAsync();
        var passwordNonce = await AddPasswordNonce(user.Id);

        var cmd = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.Login)!.Code.Value,
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
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
        nonce.Stage.Should().Be(LoginNonceStage.CodeVerified);
        (await _loginNonceCache.GetNonceAsync(passwordNonce.Id, CancellationToken.None)).Should().BeNull();
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
        var passwordNonce = await AddPasswordNonce(user.Id);

        var cmd = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = "InvalidCode",
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act
        var result = await _commandHandler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Description.Should().Contain("Invalid verification code");
    }

    [Fact]
    public async Task Handle_Given_Invalid_User_Id_Should_Return_Error()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("test??KL99"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        Db.User.Add(user);
        await Db.SaveChangesAsync();
        var unknownUserId = Guid.NewGuid();
        var passwordNonce = await AddPasswordNonce(unknownUserId);

        var cmd = new VerifyLoginCommand()
        {
            UserId = unknownUserId,
            LoginVerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.Login)!.Code.Value,
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act
        var result = await _commandHandler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Description.Should().Contain("Could not find entity with ID");
    }

    [Fact]
    public async Task Handle_Given_No_Password_Nonce_Should_Return_Unauthorized_And_Not_Count_Attempt()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("test??KL99"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        Db.User.Add(user);
        await Db.SaveChangesAsync();

        var cmd = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = "000000",
            NonceId = Guid.NewGuid(),
            Nonce = "nonce",
        };

        // Act
        var result = await _commandHandler.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        Db.VerificationCode.AsNoTracking().First(vc => vc.UserId == user.Id).Code.FailedAttempts.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Given_Password_Nonce_Twice_Should_Return_Unauthorized_On_Second_Use()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("test??KL99"), true);
        user.AddVerificationCode(UserVerificationCodeType.Login);

        Db.User.Add(user);
        await Db.SaveChangesAsync();
        var passwordNonce = await AddPasswordNonce(user.Id);

        var cmd = new VerifyLoginCommand()
        {
            UserId = user.Id,
            LoginVerificationCode = user.GetActiveVerificationCode(UserVerificationCodeType.Login)!.Code.Value,
            NonceId = passwordNonce.Id,
            Nonce = passwordNonce.NonceValue,
        };

        // Act
        var first = await _commandHandler.Handle(cmd, CancellationToken.None);
        var second = await _commandHandler.Handle(cmd, CancellationToken.None);

        // Assert
        first.Success.Should().BeTrue();
        second.Success.Should().BeFalse();
        second.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
        await _loginNonceCache.DeteleNonceAsync(first.Value!.NonceId, CancellationToken.None);
    }
}
