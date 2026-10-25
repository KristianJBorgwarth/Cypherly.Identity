using Identity.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.ModelConfigurations;

public class TokenIssuerKeyModelConfiguration : BaseModelConfiguration<TokenIssuerKey>
{
    public override void Configure(EntityTypeBuilder<TokenIssuerKey> builder)
    {
        builder.ToTable("token_issuer_key");

        builder.HasKey(k => k.Id);

        builder.Ignore(k => k.DomainEvents);

        builder.Property(k => k.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(k => k.PublicKey)
            .HasColumnName("public_key")
            .IsRequired();

        builder.Property(k => k.PrivateKey)
            .HasColumnName("private_key")
            .IsRequired();

        builder.Property(k => k.IsCurrent)
            .HasColumnName("is_current")
            .IsRequired();

        builder.HasIndex(k => k.IsCurrent)
            .HasDatabaseName("idx_token_issuer_key_is_current")
            .IsUnique()
            .HasFilter("is_current");

        base.Configure(builder);
    }
}
