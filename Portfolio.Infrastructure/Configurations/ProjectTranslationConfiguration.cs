using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class ProjectTranslationConfiguration : IEntityTypeConfiguration<ProjectTranslation>
{
    public void Configure(EntityTypeBuilder<ProjectTranslation> builder)
    {
        builder.ToTable("ProjectTranslations", table =>
            table.HasCheckConstraint(
                "CK_ProjectTranslations_LanguageCode",
                "[LanguageCode] IN ('es-ES', 'en-US')"));

        builder.HasKey(translation => new { translation.ProjectId, translation.LanguageCode });

        builder.Property(translation => translation.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(translation => translation.Title)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(translation => translation.Slug)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(translation => translation.Summary)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(translation => translation.Description)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(translation => new { translation.LanguageCode, translation.Slug })
            .IsUnique();

        builder.HasOne(translation => translation.Project)
            .WithMany(project => project.Translations)
            .HasForeignKey(translation => translation.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
