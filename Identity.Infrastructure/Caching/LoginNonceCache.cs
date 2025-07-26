using System.Text.Json;
using Identity.Application.Caching.LoginNonce;
using Identity.Application.Contracts.Cache;

namespace Identity.Infrastructure.Caching;

public class LoginNonceCache(IValkeyCacheService valkeyCacheService) : ILoginNonceCache
{
    private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        Converters = { new LoginNonceJsonConverter() },
    };

    public async Task AddNonceAsync(LoginNonce loginNonce, CancellationToken cancellationToken)
    {
        await valkeyCacheService.SetAsync(loginNonce.Id.ToString(), loginNonce, cancellationToken, TimeSpan.FromMinutes(10));
    }
    public Task<LoginNonce?> GetNonceAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        return valkeyCacheService.GetAsync<LoginNonce>(nonceId.ToString(), _options, cancellationToken);
    }
    public Task DeteleNonceAsync(Guid nonceId, CancellationToken cancellationToken)
    {
        return valkeyCacheService.RemoveAsync(nonceId.ToString(), cancellationToken);
    }
}