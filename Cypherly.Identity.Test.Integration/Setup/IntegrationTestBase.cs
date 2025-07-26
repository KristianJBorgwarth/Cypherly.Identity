using AutoFixture;
using Cypherly.Identity.Infrastructure.Caching;
using Cypherly.Identity.Infrastructure.Persistence.Context;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cypherly.Authentication.Test.Integration.Setup;

[Collection("AuthenticationApplication")]
public class IntegrationTestBase : IDisposable
{
    protected readonly IdentityDbContext Db;
    protected readonly HttpClient Client;
    protected readonly ITestHarness Harness;
    protected readonly IValkeyCacheService Cache;
    protected readonly Fixture Fixture = new Fixture();

    public IntegrationTestBase(IntegrationTestFactory<Program, IdentityDbContext> factory)
    {
        Harness = factory.Services.GetTestHarness();
        var scope = factory.Services.CreateScope();
        Db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        Cache = scope.ServiceProvider.GetRequiredService<IValkeyCacheService>();
        Db.Database.EnsureCreated();
        Client = factory.CreateClient();
        Harness.Start();
    }

    public void Dispose()
    {
        Db.VerificationCode.ExecuteDelete();
        Db.User.ExecuteDelete();
        Db.OutboxMessage.ExecuteDelete();
        Db.Device.ExecuteDelete();
        Harness.Stop();
    }
}