using FluentAssertions;
using Identity.Application.Caching;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.Authentication.Commands.VerifyNonce;
using Identity.Application.Interfaces;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;
using Identity.Infrastructure.Persistence.Context;
using Identity.Test.Integration.Setup;
using Identity.Test.Integration.Setup.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Integration.AuthenticationTest.CommandTest.VerifyNonce;

public class VerifyNonceCommandHandlerTest : IntegrationTestBase
{
    private readonly VerifyNonceCommandHandler _sut;
    public VerifyNonceCommandHandlerTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var nonceCache = scope.ServiceProvider.GetRequiredService<INonceCacheService>();
        var jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
        var verifyNonceService = scope.ServiceProvider.GetRequiredService<IVerifyNonceService>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<VerifyNonceCommandHandler>>();

        _sut = new VerifyNonceCommandHandler(repo, nonceCache, jwtService, verifyNonceService, uow, logger);
    }

    [Fact]
    public async Task Handle_Given_Command_With_Invalid_UserId_Should_Return_Result_Fail()
    {
        // Arrange
        var cmd = new VerifyNonceCommand()
        {
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
            NonceId = Guid.NewGuid(),
            Nonce = "nonce",
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Error.NotFound<User>(cmd.UserId.ToString()));
    }

    [Fact]
    public async Task Handle_Given_Command_With_Invalid_NonceID_Should_Return_Result_Fail()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("test=?lk293K"), true);
        var cmd = new VerifyNonceCommand()
        {
            UserId = user.Id,
            DeviceId = Guid.NewGuid(),
            NonceId = Guid.NewGuid(),
            Nonce = "nonce",
        };

        Db.User.Add(user);
        await Db.SaveChangesAsync();

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Error.NotFound<Nonce>(cmd.NonceId.ToString()));
    }

    [Fact]
    public async Task Handle_Given_Command_With_Invalid_Nonce_Should_Return_Result_Fail()
    {
        // Arrange
        const string publicKey = "VlmK9Smh3RVtT7CHaHW5rbrYAWeM9ImVdP6WhmnMqK0=";
        const string privateKey = "mR6AP1dNY1eEp7Z7bn6q0gPiOvcDl3FX4th65LY3Zwg=";

        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("test=?lk293K"), true);
        var device = new Device(Guid.NewGuid(), publicKey, "1.0", DeviceType.Desktop, DevicePlatform.Windows, user.Id);
        user.AddDevice(device);

        // add nonce to cache
        var nonce = Nonce.Create(user.Id, device.Id);
        await Cache.SetAsync(nonce.Id.ToString(), nonce, CancellationToken.None, TimeSpan.FromMinutes(10));

        // add user to db
        Db.User.Add(user);
        await Db.SaveChangesAsync();

        var signature = Ed25519Helper.SignNonce(nonce.NonceValue, privateKey);

        var cmd = new VerifyNonceCommand
        {
            UserId = user.Id,
            DeviceId = device.Id,
            NonceId = nonce.Id,
            Nonce = "",
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Error.Unauthorized());
        Db.RefreshToken.Count().Should().Be(0);
    }

    [Fact]
    public async Task Handle_Given_Command_With_Valid_Nonce_Should_Return_Result_Ok()
    {
        // Arrange
        const string publicKey = "VlmK9Smh3RVtT7CHaHW5rbrYAWeM9ImVdP6WhmnMqK0=";
        const string privateKey = "mR6AP1dNY1eEp7Z7bn6q0gPiOvcDl3FX4th65LY3Zwg=";

        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("test=?lk293K"), true);
        var device = new Device(Guid.NewGuid(), publicKey, "1.0", DeviceType.Desktop, DevicePlatform.Windows, user.Id);
        user.AddDevice(device);

        // add nonce to cache
        var nonce = Nonce.Create(user.Id, device.Id);
        await Cache.SetAsync(nonce.Id.ToString(), nonce, CancellationToken.None, TimeSpan.FromMinutes(10));

        // add user to db
        Db.User.Add(user);
        await Db.SaveChangesAsync();

        var signature = Ed25519Helper.SignNonce(nonce.NonceValue, privateKey);

        var cmd = new VerifyNonceCommand
        {
            UserId = user.Id,
            DeviceId = device.Id,
            NonceId = nonce.Id,
            Nonce = signature,
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Jwt.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        Db.RefreshToken.Count().Should().Be(1);
        Db.OutboxMessage.Count().Should().Be(1);
    }
}
