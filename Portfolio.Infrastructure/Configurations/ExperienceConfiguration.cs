using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class ExperienceConfiguration : IEntityTypeConfiguration<Experience>
{
    public void Configure(EntityTypeBuilder<Experience> builder)
    {
        builder.ToTable("Experiences");
        builder.HasKey(experience => experience.Id);
        builder.Property(experience => experience.RoleTitle).HasMaxLength(200).IsRequired();
        builder.Property(experience => experience.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(experience => experience.Summary).HasMaxLength(1000).IsRequired();
        builder.Property(experience => experience.StartDate).HasColumnType("date");
        builder.Property(experience => experience.EndDate).HasColumnType("date");
    }
}