using Identity.Application.Contracts.Cache;
using Identity.Infrastructure.Caching;
using Identity.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Extensions;

internal static class CachingExtensions
{
    internal static void AddValkey(this IServiceCollection services)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            var serviceProvider = services.BuildServiceProvider();
            var valkeySettings = serviceProvider.GetRequiredService<IOptions<ValkeySettings>>().Value;

            // Construct the connection string from ValkeySettings
            options.Configuration = $"{valkeySettings.Host}:{valkeySettings.Port}";
            options.InstanceName = "Cypherly.Authentication.API_";
        });

        services.AddCacheServices();
    }

    private static void AddCacheServices(this IServiceCollection services)
    {
        services.AddSingleton<IValkeyCacheService, ValkeyCacheService>();
        services.AddScoped<INonceCacheService, NonceCacheService>();
        services.AddScoped<ILoginNonceCache, LoginNonceCache>();

    }
}