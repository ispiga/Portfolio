using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class BlogPostImageConfiguration : IEntityTypeConfiguration<BlogPostImage>
{
    public void Configure(EntityTypeBuilder<BlogPostImage> builder)
    {
        builder.ToTable("BlogPostImages");
        builder.HasKey(image => image.Id);
        builder.Property(image => image.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(image => image.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(image => image.SizeBytes).IsRequired();
        builder.Property(image => image.CreatedAt).IsRequired();
        builder.HasIndex(image => new { image.BlogPostId, image.CreatedAt });
        builder.HasOne(image => image.BlogPost)
            .WithMany(post => post.Images)
            .HasForeignKey(image => image.BlogPostId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
