using Identity.Application.Contracts.Repository;
using Identity.Application.Features.Device.Queries.GetConnectionIdByUser;
using FakeItEasy;
using FluentAssertions;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Cypherly.Authentication.Test.Unit.DeviceTest.QueryTest.GetConnectionIdsByUserTest;

public class GetConnectionIdsByUserQueryHandlerTest
{
    private readonly IUserRepository _fakeRepository;
    private readonly GetConnectionIdsByUserQueryHandler _sut;

    public GetConnectionIdsByUserQueryHandlerTest()
    {
        var fakeLogger = A.Fake<ILogger<GetConnectionIdsByUserQueryHandler>>();
        _fakeRepository = A.Fake<IUserRepository>();
        _sut = new GetConnectionIdsByUserQueryHandler(_fakeRepository, fakeLogger);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var query = new GetConnectionIdsByUserQuery { UserId = Guid.NewGuid() };
        A.CallTo(() => _fakeRepository.GetByIdAsync(query.UserId)).Returns((User)null);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.NotFound(query.UserId));
    }

    [Fact]
    public async Task Handle_WhenUserFound_ReturnsConnectionIds()
    {
        // Arrange
        var query = new GetConnectionIdsByUserQuery { UserId = Guid.NewGuid() };
        var user = new User();
        user.AddDevice(new Device(Guid.NewGuid(), "test", "1.0", DeviceType.Desktop, DevicePlatform.Android, user.Id));
        A.CallTo(() => _fakeRepository.GetByIdAsync(query.UserId)).Returns(user);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.ConnectionIds.Should().Contain(x => x == user.Devices.First().ConnectionId);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ReturnsUnspecifiedError()
    {
        // Arrange
        var query = new GetConnectionIdsByUserQuery { UserId = Guid.NewGuid() };
        A.CallTo(() => _fakeRepository.GetByIdAsync(query.UserId)).Throws<Exception>();

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().BeEquivalentTo(Errors.General.UnspecifiedError("An exception occurred while retrieving connection ids for user"));
    }

    [Fact]
    public async Task Handle_When_User_Has_No_Devices_Should_Return_EmptyList()
    {
        // Arrange
        var query = new GetConnectionIdsByUserQuery { UserId = Guid.NewGuid() };
        var user = new User();

        A.CallTo(() => _fakeRepository.GetByIdAsync(query.UserId)).Returns(user);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.ConnectionIds.Should().BeEmpty();
    }
}