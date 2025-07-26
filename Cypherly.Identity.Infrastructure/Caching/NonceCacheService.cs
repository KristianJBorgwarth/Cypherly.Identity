using System.Text.Json;
using Cypherly.Identity.Application.Caching;
using Cypherly.Identity.Application.Contracts;
using Cypherly.Identity.Application.Contracts.Cache;

namespace Cypherly.Identity.Infrastructure.Caching;

public class NonceCacheService(IValkeyCacheService valkeyCacheService) : INonceCacheService
{
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

    public Task DeteleNonceAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        return valkeyCacheService.RemoveAsync(nonceId.ToString(), cancellationToken);
    }
}