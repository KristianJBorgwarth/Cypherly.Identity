using System.Text.Json;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;
using StackExchange.Redis;

namespace Identity.Infrastructure.Caching;

public class LoginNonceCache(
    IValkeyCacheService valkeyCacheService,
    IConnectionMultiplexer connectionMultiplexer)
    : ILoginNonceCache
{
    private static readonly TimeSpan ClaimLifetime = TimeSpan.FromMinutes(10);

    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new LoginNonceJsonConverter() },
    };

    public async Task AddNonceAsync(LoginNonce loginNonce, CancellationToken cancellationToken)
    {
        await valkeyCacheService.SetAsync(loginNonce.Id.ToString(), loginNonce, cancellationToken, loginNonce.ExpiresAt - DateTime.UtcNow);
    }

    public async Task<bool> TryClaimNonceAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        // SET NX is atomic, so exactly one request wins the claim. The claim outlives the nonce, so it can't be reused.
        return await connectionMultiplexer.GetDatabase()
            .StringSetAsync(ClaimKey(nonceId), "1", ClaimLifetime, When.NotExists);
    }

    public async Task ReleaseNonceClaimAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        await connectionMultiplexer.GetDatabase().KeyDeleteAsync(ClaimKey(nonceId));
    }

    private static string ClaimKey(Guid nonceId) => $"login-nonce:claim:{nonceId}";
    public Task<LoginNonce?> GetNonceAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        return valkeyCacheService.GetAsync<LoginNonce>(nonceId.ToString(), _options, cancellationToken);
    }
    public Task DeteleNonceAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        return valkeyCacheService.RemoveAsync(nonceId.ToString(), cancellationToken);
    }
}
