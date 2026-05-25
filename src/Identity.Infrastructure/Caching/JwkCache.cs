using System.Text.Json;
using Identity.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;

internal sealed class JwkCache(
    HybridCache hybridCache,
    IRsaKeyGenerator rsaKeyGenerator)
    : IJwkCache
{
    private readonly HybridCacheEntryOptions _cacheEntryOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(15),
        LocalCacheExpiration = TimeSpan.FromMinutes(5)
    };

    public async Task<JsonWebKeySet> GetJwks(CancellationToken ct = default)
    {
        var json = await hybridCache.GetOrCreateAsync(
            "jwks",
            rsaKeyGenerator,
            static (rsaGen, ct) =>
            {
                var rsaKey = rsaGen.GenerateKey();
                var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaKey);
                jwk.Alg = SecurityAlgorithms.RsaSha256;
                jwk.Use = "sig";
                var jwks = new JsonWebKeySet();
                jwks.Keys.Add(jwk);
                return ValueTask.FromResult(JsonSerializer.Serialize(jwks));
            },
            _cacheEntryOptions,
            cancellationToken: ct);

        return new JsonWebKeySet(json);
    }
}
