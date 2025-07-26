using Cypherly.Domain.Common;
using Cypherly.Identity.Application.Caching;
using Cypherly.Identity.Application.Contracts;
using Cypherly.Identity.Application.Contracts.Cache;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Application.Features.Authentication.Commands.VerifyNonce;
using Cypherly.Identity.Application.Features.Authentication.Token;
using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Domain.Common;
using Cypherly.Identity.Domain.Entities;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.ValueObjects;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace Cypherly.Authentication.Test.Unit.AuthenticationTest.CommandTest.VerifyNonce;

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

        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId)).Returns((User)null!);

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

        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId)).Returns(user);
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

        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId)).Returns(user);
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

        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId)).Returns(user);
        A.CallTo(() => _fakeCachce.GetNonceAsync(cmd.NonceId, A<CancellationToken>._)).Returns(nonce);

        A.CallTo(() => _fakeVerify.VerifyNonce(nonce.NonceValue, cmd.Nonce, device.PublicKey)).Returns(true);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_When_Exception_Should_Return_ResultFail()
    {
        // Arrange
        var cmd = new VerifyNonceCommand
        {
            DeviceId = Guid.NewGuid(),
            Nonce = "nonce",
            NonceId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        A.CallTo(() => _fakeRepo.GetByIdAsync(cmd.UserId)).Throws<Exception>();

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be(Errors.General.UnspecifiedError("An exception occured attempting to verify nonce.").Message);
    }
}