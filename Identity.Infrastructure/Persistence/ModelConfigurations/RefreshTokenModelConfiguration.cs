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
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(rt => rt.Token)
            .HasMaxLength(128)
            .HasColumnName("token")
            .IsRequired();

        builder.Property(rt => rt.Expires)
            .HasColumnName("expires")
            .IsRequired();

        builder.Property(rt => rt.Revoked)
            .HasColumnName("revoked");

        builder.Property(rt => rt.DeviceId)
            .HasColumnName("device_id")
            .IsRequired();

        builder.HasIndex(rt => rt.Token)
            .HasDatabaseName("idx_token")
            .IsUnique();
    }
}