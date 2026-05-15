using Identity.Application.Abstractions;
using Identity.Domain.Abstractions;

namespace Identity.Application.Contracts.Repository;

public interface IRepository<T> where T : AggregateRoot
{
    Task<T?> GetSinleAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task<List<T>> GetListAsync(ISpecification<T> spec, CancellationToken ct = default);
    Task CreateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
}
