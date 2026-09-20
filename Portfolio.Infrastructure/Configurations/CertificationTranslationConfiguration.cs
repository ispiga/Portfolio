using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class CertificationTranslationConfiguration : IEntityTypeConfiguration<CertificationTranslation>
{
    public void Configure(EntityTypeBuilder<CertificationTranslation> builder)
    {
        builder.ToTable("CertificationTranslations", table =>
            table.HasCheckConstraint(
                "CK_CertificationTranslations_LanguageCode",
                "[LanguageCode] IN ('es-ES', 'en-US')"));

        builder.HasKey(translation => new { translation.CertificationId, translation.LanguageCode });

        builder.Property(translation => translation.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(translation => translation.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(translation => translation.Issuer)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne(translation => translation.Certification)
            .WithMany(certification => certification.Translations)
            .HasForeignKey(translation => translation.CertificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}