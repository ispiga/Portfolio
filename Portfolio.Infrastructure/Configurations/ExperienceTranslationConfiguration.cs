using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class ExperienceTranslationConfiguration : IEntityTypeConfiguration<ExperienceTranslation>
{
    public void Configure(EntityTypeBuilder<ExperienceTranslation> builder)
    {
        builder.ToTable("ExperienceTranslations", table =>
            table.HasCheckConstraint(
                "CK_ExperienceTranslations_LanguageCode",
                "[LanguageCode] IN ('es-ES', 'en-US')"));

        builder.HasKey(translation => new { translation.ExperienceId, translation.LanguageCode });

        builder.Property(translation => translation.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(translation => translation.RoleTitle)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(translation => translation.CompanyName)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(translation => translation.Summary)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(translation => translation.Experience)
            .WithMany(experience => experience.Translations)
            .HasForeignKey(translation => translation.ExperienceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}