using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Aggregates;
using Identity.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public class TokenIssuerKeyRepository(IdentityDbContext context) : ITokenIssuerKeyRepository
{
    public Task<TokenIssuerKey?> GetCurrentAsync(CancellationToken ct = default)
    {
        return context.TokenIssuerKey.AsNoTracking().FirstOrDefaultAsync(k => k.IsCurrent, ct);
    }

    public async Task CreateAsync(TokenIssuerKey entity, CancellationToken ct = default)
    {
        await context.TokenIssuerKey.AddAsync(entity, ct);
    }

    public Task UpdateAsync(TokenIssuerKey entity, CancellationToken ct = default)
    {
        context.TokenIssuerKey.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<TokenIssuerKey?> GetSinleAsync(ISpecification<TokenIssuerKey> spec, CancellationToken ct = default)
    {
        var q = context.TokenIssuerKey.Where(spec.Criteria);

        q = spec.Includes.Aggregate(q, (current, include) => current.Include(include));

        return await q.FirstOrDefaultAsync(ct);
    }

    public Task<List<TokenIssuerKey>> GetListAsync(ISpecification<TokenIssuerKey> spec, CancellationToken ct = default)
    {
        var q = context.TokenIssuerKey.Where(spec.Criteria);

        q = spec.Includes.Aggregate(q, (current, include) => current.Include(include));

        return q.ToListAsync(ct);
    }

    public void Delete(TokenIssuerKey entity)
    {
        context.TokenIssuerKey.Remove(entity);
    }
}
