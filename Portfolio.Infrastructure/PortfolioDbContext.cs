using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Identity;

namespace Portfolio.Infrastructure;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : IdentityDbContext<PortfolioUser>(options)
{
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectTranslation> ProjectTranslations => Set<ProjectTranslation>();

    public DbSet<Experience> Experiences => Set<Experience>();

    public DbSet<ExperienceTranslation> ExperienceTranslations => Set<ExperienceTranslation>();

    public DbSet<ExperienceAttachment> ExperienceAttachments => Set<ExperienceAttachment>();

    public DbSet<Certification> Certifications => Set<Certification>();

    public DbSet<CertificationAttachment> CertificationAttachments => Set<CertificationAttachment>();

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    public DbSet<BlogPostTranslation> BlogPostTranslations => Set<BlogPostTranslation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);
    }
}