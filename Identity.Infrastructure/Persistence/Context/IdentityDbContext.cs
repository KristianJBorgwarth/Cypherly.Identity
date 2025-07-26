using Identity.Domain.Aggregates;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence.ModelConfigurations;
using Identity.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Context;

public class IdentityDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> User { get; private set; } = null!;
    public DbSet<UserVerificationCode> VerificationCode { get; private set; } = null!;
    public DbSet<Device> Device { get; private set; } = null!;
    public DbSet<RefreshToken> RefreshToken { get; private set; } = null!;
    public DbSet<OutboxMessage> OutboxMessage { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}