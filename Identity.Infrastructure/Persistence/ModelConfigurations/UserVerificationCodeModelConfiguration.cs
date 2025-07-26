using Identity.Domain.Aggregates;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.ModelConfigurations;

public class UserVerificationCodeModelConfiguration : IEntityTypeConfiguration<UserVerificationCode>
{
    public void Configure(EntityTypeBuilder<UserVerificationCode> builder)
    {
        builder.ToTable("UserVerificationCode");

        builder.HasKey(vc => vc.Id);

        builder.Property(vc => vc.Id)
            .ValueGeneratedNever();

        builder.Property(vc => vc.CodeType)
            .HasConversion<string>()
            .IsRequired();

        builder.OwnsOne(u => u.Code, uvc =>
        {
            uvc.Property(v => v.Value)
                .HasColumnName("Code")
                .HasMaxLength(20)
                .IsRequired();

            uvc.Property(v => v.ExpirationDate)
                .IsRequired();

            uvc.Property(v => v.IsUsed)
                .IsRequired();

            uvc.HasIndex(v => v.Value);

            uvc.HasIndex(v => v.ExpirationDate);
        });

        builder.HasOne<User>()
            .WithMany(u => u.VerificationCodes)
            .HasForeignKey(vc => vc.UserId);

        builder.HasIndex(vc => vc.UserId);
    }
}