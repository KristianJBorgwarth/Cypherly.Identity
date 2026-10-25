using Identity.Domain.Aggregates;

namespace Identity.Application.Contracts.Repository;

public interface ITokenIssuerKeyRepository : IRepository<TokenIssuerKey>
{
    Task<TokenIssuerKey?> GetCurrentAsync(CancellationToken ct = default);
}
