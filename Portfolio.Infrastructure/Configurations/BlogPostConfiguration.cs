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
        builder.Property(post => post.Title).HasMaxLength(200).IsRequired();
        builder.Property(post => post.Slug).HasMaxLength(200).IsRequired();
        builder.Property(post => post.Excerpt).HasMaxLength(500).IsRequired();
        builder.Property(post => post.Content).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(post => post.PublishedOn).HasColumnType("datetimeoffset");
        builder.HasIndex(post => post.Slug).IsUnique();
    }
}