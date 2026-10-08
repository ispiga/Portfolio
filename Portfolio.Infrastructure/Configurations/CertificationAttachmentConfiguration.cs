using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class CertificationAttachmentConfiguration : IEntityTypeConfiguration<CertificationAttachment>
{
    public void Configure(EntityTypeBuilder<CertificationAttachment> builder)
    {
        builder.ToTable("CertificationAttachments");
        builder.HasKey(attachment => attachment.Id);
        builder.Property(attachment => attachment.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(attachment => attachment.DisplayName).HasMaxLength(255).IsRequired();
        builder.Property(attachment => attachment.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(attachment => attachment.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(attachment => attachment.SizeBytes).IsRequired();
        builder.Property(attachment => attachment.CreatedAt).IsRequired();
        builder.Property(attachment => attachment.IsPublic).IsRequired();
        builder.HasIndex(attachment => new { attachment.CertificationId, attachment.CreatedAt });
        builder.HasOne(attachment => attachment.Certification)
            .WithMany(certification => certification.Attachments)
            .HasForeignKey(attachment => attachment.CertificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
