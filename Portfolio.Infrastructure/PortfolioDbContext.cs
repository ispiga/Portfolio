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

    public DbSet<BlogPostImage> BlogPostImages => Set<BlogPostImage>();

    public DbSet<BlogPostTranslation> BlogPostTranslations => Set<BlogPostTranslation>();

    public DbSet<HeroContent> HeroContents => Set<HeroContent>();

    public DbSet<HeroTranslation> HeroTranslations => Set<HeroTranslation>();

    public DbSet<TechnologyLogo> TechnologyLogos => Set<TechnologyLogo>();

    public DbSet<AboutProfile> AboutProfiles => Set<AboutProfile>();

    public DbSet<AboutProfileTranslation> AboutProfileTranslations => Set<AboutProfileTranslation>();

    public DbSet<SkillGroup> SkillGroups => Set<SkillGroup>();

    public DbSet<SkillGroupTranslation> SkillGroupTranslations => Set<SkillGroupTranslation>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<SkillTranslation> SkillTranslations => Set<SkillTranslation>();

    public DbSet<Hobby> Hobbies => Set<Hobby>();

    public DbSet<HobbyTranslation> HobbyTranslations => Set<HobbyTranslation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortfolioDbContext).Assembly);
    }
}