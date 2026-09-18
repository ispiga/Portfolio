using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ToTable("Certifications");
        builder.HasKey(certification => certification.Id);
        builder.Property(certification => certification.Name).HasMaxLength(200).IsRequired();
        builder.Property(certification => certification.Issuer).HasMaxLength(200).IsRequired();
        builder.Property(certification => certification.IssuedOn).HasColumnType("date");
        builder.Property(certification => certification.CredentialUrl).HasMaxLength(500);
    }
}