using Cypherly.Message.Contracts.Messages.Client;
using Cypherly.Message.Contracts.Messages.Profile;
using Cypherly.Message.Contracts.Responses.Client;
using Cypherly.Message.Contracts.Responses.Profile;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Identity.API.Filters;
using Identity.Application.Features.Authentication.Token;
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
using Testcontainers.PostgreSql;

// ReSharper disable ClassNeverInstantiated.Global

namespace Identity.Test.Integration.Setup;

public class IntegrationTestFactory<TProgram, TDbContext> : WebApplicationFactory<TProgram>, IAsyncLifetime
    where TProgram : class where TDbContext : DbContext
{
    protected bool ShouldTestWithLazyLoadingProxies { get; set; } = true;

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
            #region Database Extensions
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

                if (ShouldTestWithLazyLoadingProxies)
                {
                    options.UseLazyLoadingProxies();
                }
            });

            #endregion

            #region Auth Extensions
            // Mock out authentication and authorization for testing
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireAssertion(_ => true))
                .AddPolicy("User", policy => policy.RequireAssertion(_ => true));

            // Mock out ValidateUserIdFilter
            // Remove the existing ValidateUserIdFilter registration
            var actionFilterDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IValidateUserIdFilter));

            if (actionFilterDescriptor != null)
            {
                services.Remove(actionFilterDescriptor);
            }

            // Replace with a mock or NoOp implementation
            services.AddScoped<IValidateUserIdFilter, MockValidateUserIdIdFilter>();

            #endregion

            #region RabbitMq Extensions

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

            #endregion

            #region Jwt Extensions

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

            #endregion

            #region Valkey Extensions

            services.Configure<ValkeySettings>(options =>
            {
                options.Host = "localhost";  // The test container's host
                options.Port = 6974;        // The mapped port for the Redis container
            });

            #endregion
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _valkeyContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _valkeyContainer.StopAsync();
    }
}