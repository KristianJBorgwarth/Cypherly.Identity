using Cypherly.Message.Contracts.Messages.Client;
using Cypherly.Message.Contracts.Messages.Profile;
using Cypherly.Message.Contracts.Responses.Client;
using Cypherly.Message.Contracts.Responses.Profile;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Identity.Application.Settings;
using Identity.Infrastructure.Settings;
using Identity.Test.Integration.Setup.Authentication;
using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Quartz;
using Testcontainers.PostgreSql;

// ReSharper disable ClassNeverInstantiated.Global

namespace Identity.Test.Integration.Setup;

public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext : DbContext
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithCleanUp(true)
        .Build();

    private readonly IContainer _valkeyContainer = new ContainerBuilder()
        .WithImage("valkey/valkey:latest")
        .WithEnvironment("ALLOW_EMPTY_PASSWORD", "yes")
        .WithExposedPort(6974)
        .WithPortBinding(6974, 6379)
        .WithCleanUp(true)
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<TDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString(),
                    b => b.MigrationsAssembly(typeof(TDbContext).Assembly.FullName));
            });

            services.Configure<QuartzOptions>(options =>
                options["quartz.dataSource.default.connectionString"] = _dbContainer.GetConnectionString());


            // Mock out authentication and authorization for testing
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireAssertion(_ => true))
                .AddPolicy("User", policy => policy.RequireAssertion(_ => true));


            var rmgDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBusControl));

            if (rmgDescriptor is not null)
                services.Remove(rmgDescriptor);

            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddHandler<CreateUserProfileMessage>(async cxt =>
                {
                    await cxt.RespondAsync(new CreateUserProfileResponse
                    {
                        CorrelationId = Guid.NewGuid()
                    });
                });

                cfg.AddHandler<CreateClientMessage>(async cxt =>
                {
                    await cxt.RespondAsync(new CreateClientResponse
                    {
                        CorrelationId = Guid.NewGuid()
                    });
                });
            });

            services.RemoveAll(typeof(IConfigureOptions<JwtSettings>));

            var inMemorySettings = new Dictionary<string, string>()
            {
                { "Jwt:Secret", "SuperSecretJwtKeyForTestingOnly!@1234567890" },
                { "Jwt:Issuer", "Cypherly.Authentication.Test" },
                { "Jwt:Audience", "Test" },
                { "Jwt:TokenLifeTimeInMinutes", "20" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));


            services.Configure<ValkeySettings>(options =>
            {
                options.Host = "localhost";  // The test container's host
                options.Port = 6974;        // The mapped port for the Redis container
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _valkeyContainer.StartAsync();

        // Quartz validates its schema the moment the host starts, so the qrtz_ tables have to be
        // in place before the first test resolves the factory's services. They only exist in a
        // migration - EnsureCreated builds from the model and would skip them.
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString(),
                b => b.MigrationsAssembly(typeof(TDbContext).Assembly.FullName))
            .Options;

        await using var context = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
        await context.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _valkeyContainer.StopAsync();
    }
}
