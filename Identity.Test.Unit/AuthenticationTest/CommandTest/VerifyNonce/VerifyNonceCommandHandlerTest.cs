using FakeItEasy;
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
using Microsoft.Extensions.Logging;

namespace Identity.Test.Unit.AuthenticationTest.CommandTest.VerifyNonce;

public class VerifyNonceCommandHandlerTest
{
    private readonly IUserRepository _fakeRepo;
    private readonly INonceCacheService _fakeCachce;
    private readonly IJwtService _fakeJwt;
    private readonly IVerifyNonceService _fakeVerify;
    private readonly IUnitOfWork _fakeUnit;

    private readonly VerifyNonceCommandHandler _sut;

    public VerifyNonceCommandHandlerTest()
    {
        _fakeCachce = A.Fake<INonceCacheService>();
        _fakeJwt = A.Fake<IJwtService>();
        _fakeRepo = A.Fake<IUserRepository>();
        _fakeVerify = A.Fake<IVerifyNonceService>();
        _fakeUnit = A.Fake<IUnitOfWork>();
        var fakeLogger = A.Fake<ILogger<VerifyNonceCommandHandler>>();

        _sut = new(_fakeRepo, _fakeCachce, _fakeJwt, _fakeVerify, _fakeUnit, fakeLogger);
    }

    [Fact]
    public async Task Handle_When_User_Not_Found_Should_Return_ResultFail()
    {
        // Arrange
        var cmd = new VerifyNonceCommand
        {
            DeviceId = Guid.NewGuid(),
            Nonce = "nonce",
            NonceId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithDevicesSpec>._, A<CancellationToken>._)).Returns((User)null!);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be(Errors.General.NotFound(cmd.UserId).Message);
    }

    [Fact]
    public async Task Handle_When_Nonce_Not_Found_Should_Return_ResultFail()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("test=)23kJ00a"), true);
        var device = new Device(Guid.NewGuid(), "doesntmatter", "1.0", DeviceType.Desktop,
            DevicePlatform.Windows, user.Id);
        user.AddDevice(device);

        var cmd = new VerifyNonceCommand
        {
            DeviceId = device.Id,
            Nonce = "nonce",
            NonceId = Guid.NewGuid(),
            UserId = user.Id
        };

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithDevicesSpec>._, A<CancellationToken>._)).Returns(user);
        A.CallTo(() => _fakeCachce.GetNonceAsync(cmd.NonceId, A<CancellationToken>._)).Returns((Nonce)null!);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be(Errors.General.NotFound(cmd.NonceId).Message);
    }

    [Fact]
    public async Task Handle_When_Nonce_Invalid_Should_Return_ResultFail()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("test=)23kJ00a"), true);
        var device = new Device(Guid.NewGuid(), "doesntmatter", "1.0", DeviceType.Desktop,
            DevicePlatform.Windows, user.Id);
        user.AddDevice(device);

        var cmd = new VerifyNonceCommand
        {
            DeviceId = device.Id,
            Nonce = "invalid",
            NonceId = Guid.NewGuid(),
            UserId = user.Id
        };

        var nonce = Nonce.Create(user.Id, device.Id);

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithDevicesSpec>._, A<CancellationToken>._)).Returns(user);
        A.CallTo(() => _fakeCachce.GetNonceAsync(cmd.NonceId, A<CancellationToken>._)).Returns(nonce);

        A.CallTo(() => _fakeVerify.VerifyNonce(nonce.NonceValue, cmd.Nonce, device.PublicKey)).Returns(false);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be(Errors.General.Unauthorized().Message);
    }

    [Fact]
    public async Task Handle_When_Nonce_Valid_Should_Return_Succesfull_Result()
    {
        // Arrange
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("test=)23kJ00a"), true);
        var device = new Device(Guid.NewGuid(), "doesntmatter", "1.0", DeviceType.Desktop,
            DevicePlatform.Windows, user.Id);
        user.AddDevice(device);

        var cmd = new VerifyNonceCommand
        {
            DeviceId = device.Id,
            Nonce = "invalid",
            NonceId = Guid.NewGuid(),
            UserId = user.Id
        };

        var nonce = Nonce.Create(user.Id, device.Id);

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserWithDevicesSpec>._, A<CancellationToken>._)).Returns(user);
        A.CallTo(() => _fakeCachce.GetNonceAsync(cmd.NonceId, A<CancellationToken>._)).Returns(nonce);

        A.CallTo(() => _fakeVerify.VerifyNonce(nonce.NonceValue, cmd.Nonce, device.PublicKey)).Returns(true);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }
}
