using Identity.Application.Caching.LoginNonce;

namespace Identity.Application.Contracts.Cache;

public interface ILoginNonceCache
{
    Task AddNonceAsync(LoginNonce loginNonce, CancellationToken cancellationToken);
    Task<LoginNonce?> GetNonceAsync(Guid nonceId, CancellationToken cancellationToken);
    Task DeteleNonceAsync(Guid nonceId, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically claims the nonce for one request. Returns false if another request already claimed it,
    /// so concurrent requests can't both use the same nonce.
    /// </summary>
    Task<bool> TryClaimNonceAsync(Guid nonceId, CancellationToken cancellationToken);

    /// <summary>
    /// Releases a claim so the nonce can be used again, for when the request failed before it was consumed.
    /// </summary>
    Task ReleaseNonceClaimAsync(Guid nonceId, CancellationToken cancellationToken);
}