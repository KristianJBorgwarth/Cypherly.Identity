using Cypherly.Authentication.Test.Integration.Setup;
using Cypherly.Identity.Application.Contracts;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Application.Features.Device.Queries.GetConnectionIdsByUsers;
using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Domain.Entities;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.ValueObjects;
using Cypherly.Identity.Infrastructure.Persistence.Context;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cypherly.Authentication.Test.Integration.DeviceTest.QueryTest.GetConnectionIdsByUsersTest;

public class GetConnectionIdsByUsersQueryHandlerTest : IntegrationTestBase
{
    private readonly GetConnectionIdsByUsersQueryHandler _sut;
    public GetConnectionIdsByUsersQueryHandlerTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var repo = serviceProvider.GetRequiredService<IUserRepository>();
        var logger = serviceProvider.GetRequiredService<ILogger<GetConnectionIdsByUsersQueryHandler>>();

        _sut = new GetConnectionIdsByUsersQueryHandler(repo, logger);
    }

    [Fact]
    public async Task Handle_Given_Valid_Query_And_Users_Should_ReturnConnectionIds()
    {
        // Arrange
        var user1 = new User(Guid.NewGuid(), Email.Create("2hello@mail.dk"), Password.Create("testerPass9238?"), true);
        var user2 = new User(Guid.NewGuid(), Email.Create("3hello@mail.dk"), Password.Create("testerPass9238?"), true);
        var user3 = new User(Guid.NewGuid(), Email.Create("4hello@mail.dk"), Password.Create("testerPass9238?"), true);
        var device1 = new Device(Guid.NewGuid(), "kadsa", "1.0", DeviceType.Desktop, DevicePlatform.Android, user1.Id);
        var device2 = new Device(Guid.NewGuid(), "kadsa", "1.0", DeviceType.Desktop, DevicePlatform.Android, user2.Id);
        var device3 = new Device(Guid.NewGuid(), "kadsa", "1.0", DeviceType.Desktop, DevicePlatform.Android, user3.Id);
        var device4 = new Device(Guid.NewGuid(), "kadsa", "1.0", DeviceType.Desktop, DevicePlatform.Android, user1.Id);

        user1.AddDevice(device1);
        user1.AddDevice(device4);
        user2.AddDevice(device2);
        user3.AddDevice(device3);

        await Db.User.AddRangeAsync(user1, user2, user3);
        await Db.SaveChangesAsync();

        var query = new GetConnectionIdsByUsersQuery
        {
            UserIds = new List<Guid> { user1.Id, user2.Id, user3.Id }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull().And.Subject.Should().NotBe(null);
        result.Value.ConnectionIds[user1.Id].Should().HaveCount(2);
        result.Value.ConnectionIds[user2.Id].Should().HaveCount(1);
        result.Value.ConnectionIds[user3.Id].Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Given_Valid_Query_And_NoUsers_Should_ReturnEmptyConnectionIds()
    {
        // Arrange
        var query = new GetConnectionIdsByUsersQuery
        {
            UserIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull().And.Subject.Should().NotBe(null);
        result.Value.ConnectionIds.Should().BeEmpty();
    }
}