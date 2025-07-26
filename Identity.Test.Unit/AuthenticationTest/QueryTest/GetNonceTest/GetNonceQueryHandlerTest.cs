using FakeItEasy;
using FluentAssertions;
using Identity.Application.Caching;
using Identity.Application.Contracts.Cache;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.Authentication.Queries.GetNonce;
using Identity.Domain.Aggregates;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Unit.AuthenticationTest.QueryTest.GetNonceTest;

public class GetNonceQueryHandlerTest
{
    private readonly IUserRepository _fakeRepo;
    private readonly INonceCacheService _fakeCache;
    private readonly GetNonceQueryHandler _sut;

    public GetNonceQueryHandlerTest()
    {
        _fakeRepo = A.Fake<IUserRepository>();
        _fakeCache = A.Fake<INonceCacheService>();
        var fakeLogger = A.Fake<ILogger<GetNonceQueryHandler>>();

        _sut = new GetNonceQueryHandler(_fakeRepo, _fakeCache, fakeLogger);
    }

    [Fact]
    public async void Handle_Given_Valid_Query_Should_Return_Nonce()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("test123KJ??L"), true);
        var device = new Device(Guid.NewGuid(), "SomeKey", "1.0", DeviceType.Desktop, DevicePlatform.Android, user.Id);
        user.AddDevice(device);

        A.CallTo(() => _fakeRepo.GetByIdAsync(user.Id)).Returns(user);

        var query = new GetNonceQuery()
        {
            UserId = user.Id,
            DeviceId = device.Id
        };

        // Act
        var result = await _sut.Handle(query, new CancellationToken());

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        A.CallTo(() => _fakeCache.AddNonceAsync(A<Nonce>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async void Handle_Given_Invalid_Query_Should_Return_Fail()
    {
        // Arrange
        var query = new GetNonceQuery()
        {
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid()
        };

        A.CallTo(() => _fakeRepo.GetByIdAsync(query.UserId)).Returns((User)null!);

        // Act
        var result = await _sut.Handle(query, new CancellationToken());

        // Assert
        result.Success.Should().BeFalse();
        A.CallTo(() => _fakeCache.AddNonceAsync(A<Nonce>._, A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Fact]
    public async void Handle_Given_Exception_Should_Return_Fail()
    {
        // Arrange
        var query = new GetNonceQuery()
        {
            UserId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid()
        };

        A.CallTo(() => _fakeRepo.GetByIdAsync(query.UserId)).Throws<Exception>();

        // Act
        var result = await _sut.Handle(query, new CancellationToken());

        // Assert
        result.Success.Should().BeFalse();
        A.CallTo(() => _fakeCache.AddNonceAsync(A<Nonce>._, A<CancellationToken>._)).MustNotHaveHappened();
    }
}