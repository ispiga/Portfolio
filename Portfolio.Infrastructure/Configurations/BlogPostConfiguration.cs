using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");
        builder.HasKey(post => post.Id);
        builder.Property(post => post.FeaturedImagePath)
            .HasMaxLength(500);
        builder.Property(post => post.PublishedOn).HasColumnType("datetimeoffset");
    }
}