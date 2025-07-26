using Cypherly.Identity.Application.Contracts;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Cypherly.Identity.Infrastructure.Persistence.Repositories;

public class UserRepository(IdentityDbContext context) : IUserRepository
{
    public async Task CreateAsync(User entity)
    {
        await context.User.AddAsync(entity);
    }

    public async Task<IReadOnlyCollection<User>> GetUsersAsync(Guid[] userIds)
    {
        return await context.User.Include(u => u.Devices).Where(u => userIds.Contains(u.Id)).ToListAsync();
    }

    public Task DeleteAsync(User entity)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await context.User.FindAsync(id);
    }

    public Task UpdateAsync(User entity)
    {
        context.User.Update(entity);
        return Task.CompletedTask;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await context.User.FirstOrDefaultAsync(c => c.Email.Address.Equals(email));
    }
    public Task<User?> GetByDeviceIdAsync(Guid deviceId)
    {
        return context.User.FirstOrDefaultAsync(c => c.Devices.Any(c => c.Id == deviceId));
    }
}