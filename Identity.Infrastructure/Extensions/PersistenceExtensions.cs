using System.Reflection;
using Identity.Application.Contracts.Repository;
using Identity.Infrastructure.Persistence.Context;
using Identity.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.Extensions;

internal static class PersistenceExtensions
{
    private const string ConnectionStringName = "IdentityDbConnectionString";
    internal static void AddIdentityPersistence(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString(ConnectionStringName),
                contextOptionsBuilder =>
                {
                    contextOptionsBuilder.MigrationsAssembly(assembly.FullName);
                    contextOptionsBuilder.EnableRetryOnFailure();
                })
                .UseLazyLoadingProxies();
        });

        services.AddRepositories();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
    }
}