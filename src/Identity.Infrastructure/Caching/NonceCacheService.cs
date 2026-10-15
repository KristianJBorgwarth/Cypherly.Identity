using System.Text.Json;
using Identity.Application.Caching;
using Identity.Application.Contracts.Cache;
using StackExchange.Redis;

namespace Identity.Infrastructure.Caching;

public class NonceCacheService(
    IValkeyCacheService valkeyCacheService,
    IConnectionMultiplexer connectionMultiplexer)
    : INonceCacheService
{
    private static readonly TimeSpan ClaimLifetime = TimeSpan.FromMinutes(10);

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        Converters = { new NonceJsonConverter() },
    };

    public async Task AddNonceAsync(Nonce nonce, CancellationToken cancellationToken)
    {
        await valkeyCacheService.SetAsync(nonce.Id.ToString(), nonce, cancellationToken, TimeSpan.FromMinutes(5));
    }

    public async Task<Nonce?> GetNonceAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        return await valkeyCacheService.GetAsync<Nonce>(nonceId.ToString(), _options, cancellationToken);
    }

    public Task DeleteNonceAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        return valkeyCacheService.RemoveAsync(nonceId.ToString(), cancellationToken);
    }

    public async Task<bool> TryClaimNonceAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        return await connectionMultiplexer.GetDatabase()
            .StringSetAsync(ClaimKey(nonceId), "1", ClaimLifetime, When.NotExists);
    }

    private static string ClaimKey(Guid nonceId) => $"nonce:claim:{nonceId}";
}
