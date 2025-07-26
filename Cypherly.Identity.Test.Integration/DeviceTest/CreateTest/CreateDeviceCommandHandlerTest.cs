using Cypherly.Authentication.Test.Integration.Setup;
using Cypherly.Identity.Application.Caching.LoginNonce;
using Cypherly.Identity.Application.Contracts.Cache;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Application.Features.Device.Commands.Create;
using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Domain.Common;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.Services.User;
using Cypherly.Identity.Domain.ValueObjects;
using Cypherly.Identity.Infrastructure.Persistence.Context;
using Cypherly.Message.Contracts.Messages.Client;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cypherly.Authentication.Test.Integration.DeviceTest.CreateTest;

public class CreateDeviceCommandHandlerTest : IntegrationTestBase
{
    private readonly CreateDeviceCommandHandler _sut;
    private readonly ILoginNonceCache _loginNonceCache;

    public CreateDeviceCommandHandlerTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        _loginNonceCache = scope.ServiceProvider.GetRequiredService<ILoginNonceCache>();
        var deviceService = scope.ServiceProvider.GetRequiredService<IDeviceService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var requestClient = scope.ServiceProvider.GetRequiredService<IRequestClient<CreateClientMessage>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CreateDeviceCommandHandler>>();
        _sut = new CreateDeviceCommandHandler(repo, requestClient, _loginNonceCache, deviceService, unitOfWork, logger);
    }

    [Fact]
    public async Task Handle_Given_Valid_Command_Should_Return_ResultOk_And_Create_Device()
    {
        // Arrange user
        var user = new User(Guid.NewGuid(), Email.Create("test@gmail.com"), Password.Create("test=???KKL999"), true);

        Db.User.Add(user);
        await Db.SaveChangesAsync();

        // Arrange nonce
        var loginNonce = LoginNonce.Create(user.Id);
        await _loginNonceCache.AddNonceAsync(loginNonce, CancellationToken.None);

        // Arrange command
        var cmd = new CreateDeviceCommand()
        {
            UserId = user.Id,
            LoginNonceId = loginNonce.Id,
            LoginNonce = loginNonce.NonceValue,
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Mobile,
            DevicePlatform = DevicePlatform.Android,
            Base64DevicePublicKey = "base64DevicePublicKey",
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.DeviceId.Should().NotBeEmpty();
        result.Value.DeviceConnectionId.Should().NotBeEmpty();
        Db.Device.Count().Should().Be(1);
        Harness.Published.Select<CreateClientMessage>().Where(cr => cr.Context.Message.DeviceId == Db.Device.FirstOrDefault()!.Id).Should().HaveCount(1);
        var device = Db.Device.First();
        device.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_Given_Invalid_UserId_Should_Return_ResultFail()
    {
        // Arrange
        var cmd = new CreateDeviceCommand()
        {
            UserId = Guid.NewGuid(),
            LoginNonceId = Guid.NewGuid(),
            LoginNonce = "nonce",
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Mobile,
            DevicePlatform = DevicePlatform.Android,
            Base64DevicePublicKey = "base64DevicePublicKey",
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.NotFound(cmd.UserId));
    }

    [Fact]
    public async Task Handle_Given_Invalid_LoginNonceId_Should_Return_ResultFail()
    {
        // Arrange user
        var user = new User(Guid.NewGuid(), Email.Create("test@gmail.com"), Password.Create("test=???KKL999"), true);

        Db.User.Add(user);
        await Db.SaveChangesAsync();

        // Arrange nonce
        var loginNonce = LoginNonce.Create(user.Id);
        await _loginNonceCache.AddNonceAsync(loginNonce, CancellationToken.None);

        // Arrange command
        var cmd = new CreateDeviceCommand()
        {
            UserId = user.Id,
            LoginNonceId = loginNonce.Id,
            LoginNonce = "invalidNonce",
            DeviceAppVersion = "1.0",
            DeviceType = DeviceType.Mobile,
            DevicePlatform = DevicePlatform.Android,
            Base64DevicePublicKey = "base64DevicePublicKey",
        };

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.Unauthorized());
    }
}