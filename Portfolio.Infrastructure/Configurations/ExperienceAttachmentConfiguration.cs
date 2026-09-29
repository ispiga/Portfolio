using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class ExperienceAttachmentConfiguration : IEntityTypeConfiguration<ExperienceAttachment>
{
    public void Configure(EntityTypeBuilder<ExperienceAttachment> builder)
    {
        builder.ToTable("ExperienceAttachments");
        builder.HasKey(attachment => attachment.Id);
        builder.Property(attachment => attachment.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(attachment => attachment.DisplayName).HasMaxLength(255).IsRequired();
        builder.Property(attachment => attachment.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(attachment => attachment.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(attachment => attachment.SizeBytes).IsRequired();
        builder.Property(attachment => attachment.CreatedAt).IsRequired();
        builder.Property(attachment => attachment.IsPublic).HasDefaultValue(false).IsRequired();
        builder.HasIndex(attachment => new { attachment.ExperienceId, attachment.CreatedAt });
        builder.HasOne(attachment => attachment.Experience)
            .WithMany(experience => experience.Attachments)
            .HasForeignKey(attachment => attachment.ExperienceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}