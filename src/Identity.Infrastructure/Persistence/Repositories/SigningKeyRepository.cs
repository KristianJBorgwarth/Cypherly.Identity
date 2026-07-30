using Identity.Application.Abstractions;
using Identity.Application.Contracts.Repository;
using Identity.Domain.Aggregates;
using Identity.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public class SigningKeyRepository(IdentityDbContext context) : ISigningKeyRepository
{
    /// <summary>
    /// Tracked, because the rotation job retires the key it gets back.
    /// </summary>
    public Task<SigningKey?> GetCurrentAsync(CancellationToken ct = default)
    {
        return context.SigningKey.FirstOrDefaultAsync(sk => sk.IsCurrent, ct);
    }

    /// <summary>
    /// Untracked, because this feeds the public JWKS and is never mutated.
    /// </summary>
    public async Task<IReadOnlyList<SigningKey>> GetPublishedAsync(DateTime now, CancellationToken ct = default)
    {
        return await context.SigningKey
            .AsNoTracking()
            .Where(sk => sk.IsCurrent || (sk.ExpiresAt != null && now < sk.ExpiresAt))
            .OrderByDescending(sk => sk.IsCurrent)
            .ThenByDescending(sk => sk.Created)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SigningKey>> GetPurgeableAsync(DateTime now, CancellationToken ct = default)
    {
        return await context.SigningKey
            .Where(sk => sk.IsCurrent == false && sk.ExpiresAt != null && now >= sk.ExpiresAt)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(SigningKey entity, CancellationToken ct = default)
    {
        await context.SigningKey.AddAsync(entity, ct);
    }

    public Task UpdateAsync(SigningKey entity, CancellationToken ct = default)
    {
        context.SigningKey.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<SigningKey?> GetSinleAsync(ISpecification<SigningKey> spec, CancellationToken ct = default)
    {
        var q = context.SigningKey.Where(spec.Criteria);

        q = spec.Includes.Aggregate(q, (current, include) => current.Include(include));

        return await q.FirstOrDefaultAsync(ct);
    }

    public Task<List<SigningKey>> GetListAsync(ISpecification<SigningKey> spec, CancellationToken ct = default)
    {
        var q = context.SigningKey.Where(spec.Criteria);

        q = spec.Includes.Aggregate(q, (current, include) => current.Include(include));

        return q.ToListAsync(ct);
    }

    public void Delete(SigningKey entity)
    {
        context.SigningKey.Remove(entity);
    }
}
