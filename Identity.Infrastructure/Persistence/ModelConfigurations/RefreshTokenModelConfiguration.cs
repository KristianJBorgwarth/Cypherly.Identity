using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.ModelConfigurations;

public class RefreshTokenModelConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshToken");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .ValueGeneratedNever();

        builder.Property(rt => rt.Token)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(rt => rt.Expires)
            .IsRequired();

        builder.Property(rt => rt.Revoked);

        builder.Property(rt => rt.DeviceId)
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .IsUnique();
    }
}