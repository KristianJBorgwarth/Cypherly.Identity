using Identity.Domain.Abstractions;

namespace Identity.Application.Contracts.Repository;

public interface IRepository<T> where T : AggregateRoot
{
    Task CreateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
}
