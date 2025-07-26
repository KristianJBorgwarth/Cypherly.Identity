using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Domain.Entities;
using Cypherly.Identity.Infrastructure.Persistence.ModelConfigurations;
using Cypherly.Identity.Infrastructure.Persistence.Outbox;
using Cypherly.Identity.Persistence.ModelConfigurations;
using Microsoft.EntityFrameworkCore;

namespace Cypherly.Identity.Infrastructure.Persistence.Context;

public class
    IdentityDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> User { get; private set; } = null!;
    public DbSet<UserVerificationCode> VerificationCode { get; private set; } = null!;
    public DbSet<Device> Device { get; private set; } = null!;
    public DbSet<RefreshToken> RefreshToken { get; private set; } = null!;

    public DbSet<OutboxMessage> OutboxMessage { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageModelConfiguration());
        modelBuilder.ApplyConfiguration(new UserModelConfiguration());
        modelBuilder.ApplyConfiguration(new DeviceModelConfiguration());
        modelBuilder.ApplyConfiguration(new UserVerificationCodeModelConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenModelConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}