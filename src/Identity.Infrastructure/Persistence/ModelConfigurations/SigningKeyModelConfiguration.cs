using Identity.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.ModelConfigurations;

public class SigningKeyModelConfiguration : BaseModelConfiguration<SigningKey>
{
    public override void Configure(EntityTypeBuilder<SigningKey> builder)
    {
        builder.ToTable("signing_key");

        builder.HasKey(sk => sk.Id);

        builder.Ignore(sk => sk.DomainEvents);

        builder.Property(sk => sk.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(sk => sk.Kid)
            .HasMaxLength(64)
            .HasColumnName("kid")
            .IsRequired();

        builder.Property(sk => sk.N)
            .HasColumnName("n")
            .IsRequired();

        builder.Property(sk => sk.E)
            .HasMaxLength(16)
            .HasColumnName("e")
            .IsRequired();

        builder.Property(sk => sk.Kty)
            .HasMaxLength(8)
            .HasColumnName("kty")
            .IsRequired();

        builder.Property(sk => sk.Use)
            .HasMaxLength(8)
            .HasColumnName("use")
            .IsRequired();

        builder.Property(sk => sk.Alg)
            .HasMaxLength(16)
            .HasColumnName("alg")
            .IsRequired();

        builder.Property(sk => sk.PrivateKey)
            .HasColumnName("private_key")
            .IsRequired();

        builder.Property(sk => sk.IsCurrent)
            .HasColumnName("is_current")
            .IsRequired();

        builder.Property(sk => sk.ExpiresAt)
            .HasColumnName("expires_at");

        builder.HasIndex(sk => sk.Kid)
            .HasDatabaseName("idx_signing_key_kid")
            .IsUnique();

        // Two replicas racing the rotation job cannot both leave a current key behind.
        builder.HasIndex(sk => sk.IsCurrent)
            .HasDatabaseName("idx_signing_key_is_current")
            .IsUnique()
            .HasFilter("is_current");

        builder.HasIndex(sk => sk.ExpiresAt)
            .HasDatabaseName("idx_signing_key_expires_at");

        base.Configure(builder);
    }
}
