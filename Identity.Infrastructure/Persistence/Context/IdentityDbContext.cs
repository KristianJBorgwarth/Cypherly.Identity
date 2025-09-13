using Identity.Domain.Aggregates;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence.ModelConfigurations;
using Identity.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Context;

public class IdentityDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> User { get; init; }
    public DbSet<UserVerificationCode> VerificationCode { get; init; }
    public DbSet<Device> Device { get; init; }
    public DbSet<RefreshToken> RefreshToken { get; init; }
    public DbSet<OutboxMessage> OutboxMessage { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}