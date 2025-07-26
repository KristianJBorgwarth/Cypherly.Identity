using Cypherly.Identity.Domain.Aggregates;

namespace Cypherly.Identity.Application.Contracts.Repository;

public interface IUserRepository : IRepository<User>
{
    Task<IReadOnlyCollection<User>> GetUsersAsync(Guid[] userIds);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByDeviceIdAsync(Guid deviceId);
}