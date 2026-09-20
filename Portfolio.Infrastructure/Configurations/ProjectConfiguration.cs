using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(project => project.Id);
        builder.Property(project => project.RepositoryUrl).HasMaxLength(500);
        builder.Property(project => project.DemoUrl).HasMaxLength(500);
        builder.Property(project => project.PreviewImagePath).HasMaxLength(500);
    }
}