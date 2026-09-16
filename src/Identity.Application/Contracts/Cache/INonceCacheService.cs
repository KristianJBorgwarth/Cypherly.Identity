using Identity.Application.Caching;

namespace Identity.Application.Contracts.Cache;

public interface INonceCacheService
{
    Task AddNonceAsync(Nonce nonce, CancellationToken cancellationToken);
    Task<Nonce?> GetNonceAsync(Guid nonceId, CancellationToken cancellationToken);
    Task DeleteNonceAsync(Guid nonceId, CancellationToken cancellationToken);
}
