using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Identity.Infrastructure.Caching;

[SuppressMessage("Design", "CA1068:CancellationToken parameters must come last")]
public interface IValkeyCacheService
{
    Task<T?> GetAsync<T>(string key, JsonSerializerOptions? options, CancellationToken cancellationToken);
    Task SetAsync<T>(string key, T value, CancellationToken cancellationToken, TimeSpan? expiry);
    Task RemoveAsync(string key, CancellationToken cancellationToken);
}

internal class ValkeyCacheService(IDistributedCache cache) : IValkeyCacheService
{
    public async Task<T?> GetAsync<T>(string key, JsonSerializerOptions? options, CancellationToken cancellationToken)
    {
        var serializedValue = await cache.GetStringAsync(key, cancellationToken);

        return serializedValue == null ? default : JsonSerializer.Deserialize<T>(serializedValue, options ?? null);
    }

    public async Task SetAsync<T>(string key, T value, CancellationToken cancellationToken, TimeSpan? expiry = null)
    {
        var options = new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
        };

        var serializedValue = JsonSerializer.Serialize(value);

        await cache.SetStringAsync(key, serializedValue, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        await cache.RemoveAsync(key, cancellationToken);
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken)
    {

    }
}