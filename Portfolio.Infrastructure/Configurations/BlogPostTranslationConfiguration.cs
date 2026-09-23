using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class BlogPostTranslationConfiguration : IEntityTypeConfiguration<BlogPostTranslation>
{
    public void Configure(EntityTypeBuilder<BlogPostTranslation> builder)
    {
        builder.ToTable("BlogPostTranslations", table =>
            table.HasCheckConstraint(
                "CK_BlogPostTranslations_LanguageCode",
                "[LanguageCode] IN ('es-ES', 'en-US')"));

        builder.HasKey(translation => new { translation.BlogPostId, translation.LanguageCode });

        builder.Property(translation => translation.LanguageCode)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(translation => translation.Title)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(translation => translation.Slug)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(translation => translation.Excerpt)
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(translation => translation.Content)
            .HasColumnType("nvarchar(max)")
            .IsRequired();
        builder.Property(translation => translation.FeaturedImageAlt)
            .HasMaxLength(500);

        builder.HasIndex(translation => new { translation.LanguageCode, translation.Slug })
            .IsUnique();

        builder.HasOne(translation => translation.BlogPost)
            .WithMany(blogPost => blogPost.Translations)
            .HasForeignKey(translation => translation.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
