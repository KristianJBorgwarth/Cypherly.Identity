using Identity.Domain.Aggregates;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.ModelConfigurations;

public class UserVerificationCodeModelConfiguration : BaseModelConfiguration<UserVerificationCode>
{
    public override void Configure(EntityTypeBuilder<UserVerificationCode> builder)
    {
        builder.ToTable("user_verification_code");

        builder.HasKey(vc => vc.Id);

        builder.Property(vc => vc.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(vc => vc.CodeType)
            .HasConversion<string>()
            .HasColumnName("code_type")
            .IsRequired();

        builder.OwnsOne(u => u.Code, uvc =>
        {
            uvc.Property(v => v.Value)
                .HasColumnName("code")
                .HasMaxLength(20)
                .IsRequired();

            uvc.Property(v => v.ExpirationDate)
                .HasColumnName("expiration_date")
                .IsRequired();

            uvc.Property(v => v.IsUsed)
                .HasColumnName("is_used")
                .IsRequired();

            uvc.HasIndex(v => v.Value);

            uvc.HasIndex(v => v.ExpirationDate)
                .HasDatabaseName("idx_expiration_date");
        });

        builder.HasOne<User>()
            .WithMany(u => u.VerificationCodes)
            .HasForeignKey(vc => vc.UserId);

        builder.HasIndex(vc => vc.UserId)
            .HasDatabaseName("idx_user_id");
        
        base.Configure(builder);
    }
}