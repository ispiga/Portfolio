using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure;

public sealed class PortfolioDbContext(DbContextOptions<PortfolioDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectTranslation> ProjectTranslations => Set<ProjectTranslation>();

    public DbSet<Experience> Experiences => Set<Experience>();

    public DbSet<ExperienceTranslation> ExperienceTranslations => Set<ExperienceTranslation>();

    public DbSet<Certification> Certifications => Set<Certification>();

    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();

    public DbSet<BlogPostTranslation> BlogPostTranslations => Set<BlogPostTranslation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);
    }
}