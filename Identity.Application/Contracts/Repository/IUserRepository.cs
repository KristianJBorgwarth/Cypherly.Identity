using Identity.Domain.Aggregates;

namespace Identity.Application.Contracts.Repository;

public interface IUserRepository : IRepository<User>
{
    Task<IReadOnlyCollection<User>> GetUsersAsync(Guid[] userIds, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByDeviceIdAsync(Guid deviceId, CancellationToken ct = default);
}