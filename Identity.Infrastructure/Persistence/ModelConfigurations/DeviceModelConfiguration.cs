using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.ModelConfigurations;

public class DeviceModelConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("device");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(d => d.ConnectionId)
            .IsRequired()
            .HasColumnName("connection_id")
            .ValueGeneratedNever();

        builder.Property(d => d.Name)
            .HasColumnName("name")
            .IsRequired();

        builder.Property(d => d.AppVersion)
            .HasColumnName("app_version")
            .IsRequired();

        builder.Property(d => d.PublicKey)
            .HasColumnName("public_key")
            .IsRequired();

        builder.Property(d => d.Type)
            .HasConversion<string>()
            .HasColumnName("type")
            .IsRequired();

        builder.Property(d => d.Platform)
            .HasConversion<string>()
            .HasColumnName("platform")
            .IsRequired();

        builder.HasIndex(d => d.UserId)
            .HasDatabaseName("idx_device_user_id");
    }
}