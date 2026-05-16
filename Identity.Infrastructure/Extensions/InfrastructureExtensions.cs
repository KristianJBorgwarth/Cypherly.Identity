using System.Reflection;
using Identity.Infrastructure.Interfaces;
using Identity.Infrastructure.Services;
using Identity.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, Assembly assembly)
    {
        services.AddOutboxProcessingJob(assembly);
        services.ConfigureSettings(configuration);
        services.AddIdentityPersistence(configuration, assembly);
        services.AddMassTransitRabbitMq();
        services.AddServices();
        services.AddValkey();
    }

    private static void ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ValkeySettings>(configuration.GetSection("Valkey"));
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMq"));
    }

    private static void AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IRsaKeyGenerator, RsaKeyGenerator>();
    }
}
