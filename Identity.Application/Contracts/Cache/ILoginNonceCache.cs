using Identity.Application.Caching.LoginNonce;

namespace Identity.Application.Contracts.Cache;

public interface ILoginNonceCache
{
    Task AddNonceAsync(LoginNonce loginNonce, CancellationToken cancellationToken);
    Task<LoginNonce?> GetNonceAsync(Guid nonceId, CancellationToken cancellationToken);
    Task DeteleNonceAsync(Guid nonceId, CancellationToken cancellationToken);
}