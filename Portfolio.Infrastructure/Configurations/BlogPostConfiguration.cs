using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts", table => table.HasCheckConstraint(
            "CK_BlogPosts_EditorialStatus",
            "[EditorialStatus] IN (0, 1, 2)"));
        builder.HasKey(post => post.Id);
        builder.Property(post => post.FeaturedImagePath)
            .HasMaxLength(500);
        builder.Property(post => post.PublishedOn).HasColumnType("datetimeoffset");
        builder.Property(post => post.EditorialStatus)
            .HasConversion<int>()
            .HasDefaultValue(BlogPostEditorialStatus.Draft)
            .IsRequired();
    }
}