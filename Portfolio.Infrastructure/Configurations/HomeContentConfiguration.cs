using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Configurations;

public sealed class HeroContentConfiguration : IEntityTypeConfiguration<HeroContent>
{
    public void Configure(EntityTypeBuilder<HeroContent> builder)
    {
        builder.ToTable("HeroContent", table =>
        {
            table.HasCheckConstraint("CK_HeroContent_Singleton", "[Id] = 1");
            table.HasCheckConstraint("CK_HeroContent_OrbitCount", "[OrbitCount] >= 1");
            table.HasCheckConstraint("CK_HeroContent_ProfileImage", "([ProfileImageId] IS NULL AND [ProfileImageStorageKey] IS NULL AND [ProfileImageContentType] IS NULL AND [ProfileImageSizeBytes] IS NULL AND [ProfileImageAlternativeText] IS NULL) OR ([ProfileImageId] IS NOT NULL AND [ProfileImageStorageKey] IS NOT NULL AND [ProfileImageContentType] IS NOT NULL AND [ProfileImageSizeBytes] > 0 AND [ProfileImageAlternativeText] IS NOT NULL)");
        });
        builder.HasKey(content => content.Id);
        builder.Property(content => content.Id).ValueGeneratedNever();
        builder.Property(content => content.ProfileImageStorageKey).HasMaxLength(255);
        builder.Property(content => content.ProfileImageContentType).HasMaxLength(100);
        builder.Property(content => content.ProfileImageAlternativeText).HasMaxLength(200);
        builder.HasMany(content => content.Translations)
            .WithOne(translation => translation.HeroContent)
            .HasForeignKey(translation => translation.HeroContentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class HeroTranslationConfiguration : IEntityTypeConfiguration<HeroTranslation>
{
    public void Configure(EntityTypeBuilder<HeroTranslation> builder)
    {
        builder.ToTable("HeroTranslations", table => table.HasCheckConstraint(
            "CK_HeroTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')"));
        builder.HasKey(translation => new { translation.HeroContentId, translation.LanguageCode });
        builder.Property(translation => translation.LanguageCode).HasMaxLength(10).IsRequired();
        builder.Property(translation => translation.Headline).HasMaxLength(250).IsRequired();
    }
}

public sealed class TechnologyLogoConfiguration : IEntityTypeConfiguration<TechnologyLogo>
{
    public void Configure(EntityTypeBuilder<TechnologyLogo> builder)
    {
        builder.ToTable("TechnologyLogos", table =>
        {
            table.HasCheckConstraint("CK_TechnologyLogos_DisplayOrder", "[DisplayOrder] >= 0");
            table.HasCheckConstraint("CK_TechnologyLogos_Orbit", "[Orbit] >= 1");
            table.HasCheckConstraint("CK_TechnologyLogos_SizeBytes", "[SizeBytes] > 0");
        });
        builder.HasKey(logo => logo.Id);
        builder.Property(logo => logo.Name).HasMaxLength(100).IsRequired();
        builder.Property(logo => logo.AlternativeText).HasMaxLength(200).IsRequired();
        builder.Property(logo => logo.StorageKey).HasMaxLength(255).IsRequired();
        builder.Property(logo => logo.ContentType).HasMaxLength(100).IsRequired();
        builder.HasIndex(logo => new { logo.Orbit, logo.DisplayOrder });
    }
}

public sealed class AboutProfileConfiguration : IEntityTypeConfiguration<AboutProfile>
{
    public void Configure(EntityTypeBuilder<AboutProfile> builder)
    {
        builder.ToTable("AboutProfile", table => table.HasCheckConstraint(
            "CK_AboutProfile_Singleton", "[Id] = 1"));
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Id).ValueGeneratedNever();
        builder.HasMany(profile => profile.Translations)
            .WithOne(translation => translation.AboutProfile)
            .HasForeignKey(translation => translation.AboutProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class AboutProfileTranslationConfiguration : IEntityTypeConfiguration<AboutProfileTranslation>
{
    public void Configure(EntityTypeBuilder<AboutProfileTranslation> builder)
    {
        builder.ToTable("AboutProfileTranslations", table => table.HasCheckConstraint(
            "CK_AboutProfileTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')"));
        builder.HasKey(translation => new { translation.AboutProfileId, translation.LanguageCode });
        builder.Property(translation => translation.LanguageCode).HasMaxLength(10).IsRequired();
        builder.Property(translation => translation.Description).HasMaxLength(4000).IsRequired();
    }
}

public sealed class SkillGroupConfiguration : IEntityTypeConfiguration<SkillGroup>
{
    public void Configure(EntityTypeBuilder<SkillGroup> builder)
    {
        builder.ToTable("SkillGroups", table => table.HasCheckConstraint(
            "CK_SkillGroups_DisplayOrder", "[DisplayOrder] >= 0"));
        builder.HasKey(group => group.Id);
        builder.HasIndex(group => group.DisplayOrder);
        builder.HasMany(group => group.Translations)
            .WithOne(translation => translation.SkillGroup)
            .HasForeignKey(translation => translation.SkillGroupId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(group => group.Skills)
            .WithOne(skill => skill.SkillGroup)
            .HasForeignKey(skill => skill.SkillGroupId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class SkillGroupTranslationConfiguration : IEntityTypeConfiguration<SkillGroupTranslation>
{
    public void Configure(EntityTypeBuilder<SkillGroupTranslation> builder)
    {
        builder.ToTable("SkillGroupTranslations", table => table.HasCheckConstraint(
            "CK_SkillGroupTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')"));
        builder.HasKey(translation => new { translation.SkillGroupId, translation.LanguageCode });
        builder.Property(translation => translation.LanguageCode).HasMaxLength(10).IsRequired();
        builder.Property(translation => translation.Name).HasMaxLength(100).IsRequired();
    }
}

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills", table => table.HasCheckConstraint(
            "CK_Skills_DisplayOrder", "[DisplayOrder] >= 0"));
        builder.HasKey(skill => skill.Id);
        builder.HasIndex(skill => new { skill.SkillGroupId, skill.DisplayOrder });
        builder.HasMany(skill => skill.Translations)
            .WithOne(translation => translation.Skill)
            .HasForeignKey(translation => translation.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class SkillTranslationConfiguration : IEntityTypeConfiguration<SkillTranslation>
{
    public void Configure(EntityTypeBuilder<SkillTranslation> builder)
    {
        builder.ToTable("SkillTranslations", table => table.HasCheckConstraint(
            "CK_SkillTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')"));
        builder.HasKey(translation => new { translation.SkillId, translation.LanguageCode });
        builder.Property(translation => translation.LanguageCode).HasMaxLength(10).IsRequired();
        builder.Property(translation => translation.Name).HasMaxLength(100).IsRequired();
    }
}

public sealed class HobbyConfiguration : IEntityTypeConfiguration<Hobby>
{
    public void Configure(EntityTypeBuilder<Hobby> builder)
    {
        builder.ToTable("Hobbies", table =>
        {
            table.HasCheckConstraint("CK_Hobbies_DisplayOrder", "[DisplayOrder] >= 0");
            table.HasCheckConstraint("CK_Hobbies_SizeBytes", "[SizeBytes] > 0");
        });
        builder.HasKey(hobby => hobby.Id);
        builder.Property(hobby => hobby.StorageKey).HasMaxLength(255).IsRequired();
        builder.Property(hobby => hobby.ContentType).HasMaxLength(100).IsRequired();
        builder.HasIndex(hobby => hobby.DisplayOrder);
        builder.HasMany(hobby => hobby.Translations)
            .WithOne(translation => translation.Hobby)
            .HasForeignKey(translation => translation.HobbyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class HobbyTranslationConfiguration : IEntityTypeConfiguration<HobbyTranslation>
{
    public void Configure(EntityTypeBuilder<HobbyTranslation> builder)
    {
        builder.ToTable("HobbyTranslations", table => table.HasCheckConstraint(
            "CK_HobbyTranslations_LanguageCode", "[LanguageCode] IN ('es-ES', 'en-US')"));
        builder.HasKey(translation => new { translation.HobbyId, translation.LanguageCode });
        builder.Property(translation => translation.LanguageCode).HasMaxLength(10).IsRequired();
        builder.Property(translation => translation.Name).HasMaxLength(150).IsRequired();
        builder.Property(translation => translation.Description).HasMaxLength(1200).IsRequired();
    }
}