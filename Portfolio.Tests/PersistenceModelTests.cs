using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Portfolio.Application.Blog;
using Portfolio.Application.Certifications;
using Portfolio.Application.Experiences;
using Portfolio.Application.Projects;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Xunit;

namespace Portfolio.Tests;

public sealed class PersistenceModelTests
{
    [Fact]
    public void Model_contains_only_the_initial_portfolio_entities()
    {
        using var context = CreateContext();

        var entityNames = context.Model.GetEntityTypes()
            .Select(entityType => entityType.ClrType.Name)
            .OrderBy(name => name)
            .ToArray();

        Assert.Equal(
            ["BlogPost", "BlogPostTranslation", "Certification", "CertificationTranslation", "Experience", "ExperienceTranslation", "Project", "ProjectTranslation"],
            entityNames);
    }

    [Fact]
    public void Experience_translations_have_localized_fields_and_required_constraints()
    {
        using var context = CreateContext();

        var experienceEntity = context.Model.FindEntityType(typeof(Experience))!;
        var translationEntity = context.Model.FindEntityType(typeof(ExperienceTranslation))!;

        Assert.DoesNotContain("RoleTitle", experienceEntity.GetProperties().Select(property => property.Name));
        Assert.DoesNotContain("CompanyName", experienceEntity.GetProperties().Select(property => property.Name));
        Assert.DoesNotContain("Summary", experienceEntity.GetProperties().Select(property => property.Name));
        Assert.Equal(
            [nameof(ExperienceTranslation.ExperienceId), nameof(ExperienceTranslation.LanguageCode)],
            translationEntity.FindPrimaryKey()!.Properties.Select(property => property.Name));

        var foreignKey = translationEntity.GetForeignKeys().Single();
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);

        var designTimeTranslationEntity = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(ExperienceTranslation))!;
        var languageConstraint = designTimeTranslationEntity.GetCheckConstraints()
            .Single(constraint => constraint.Name == "CK_ExperienceTranslations_LanguageCode");
        Assert.Contains("es-ES", languageConstraint.Sql);
        Assert.Contains("en-US", languageConstraint.Sql);
    }

    [Fact]
    public void Experience_translation_selector_uses_requested_culture_and_common_fields()
    {
        var experience = new Experience
        {
            Id = Guid.NewGuid(),
            StartDate = new DateOnly(2022, 1, 10),
            EndDate = new DateOnly(2024, 6, 30),
            DisplayOrder = 4,
            Translations =
            [
                new ExperienceTranslation
                {
                    LanguageCode = "es-ES",
                    RoleTitle = "Desarrollador",
                    CompanyName = "Empresa",
                    Summary = "Resumen en español"
                },
                new ExperienceTranslation
                {
                    LanguageCode = "en-US",
                    RoleTitle = "Developer",
                    CompanyName = "Company",
                    Summary = "English summary"
                }
            ]
        };

        var result = ExperienceTranslationSelector.Select(experience, "en-US");

        Assert.NotNull(result);
        Assert.Equal("Developer", result.RoleTitle);
        Assert.Equal("Company", result.CompanyName);
        Assert.Equal("English summary", result.Summary);
        Assert.Equal(experience.StartDate, result.StartDate);
        Assert.Equal(experience.EndDate, result.EndDate);
        Assert.Equal(4, result.DisplayOrder);
    }

    [Fact]
    public void Experience_translation_selector_falls_back_to_spanish()
    {
        var experience = new Experience
        {
            Translations =
            [
                new ExperienceTranslation
                {
                    LanguageCode = "es-ES",
                    RoleTitle = "Desarrollador",
                    CompanyName = "Empresa",
                    Summary = "Resumen"
                }
            ]
        };

        var result = ExperienceTranslationSelector.Select(experience, "en-US");

        Assert.NotNull(result);
        Assert.Equal("Desarrollador", result.RoleTitle);
    }

    [Fact]
    public void Experience_translation_selector_discards_experience_without_valid_translation()
    {
        var experience = new Experience
        {
            Translations =
            [
                new ExperienceTranslation
                {
                    LanguageCode = "en-US",
                    RoleTitle = "Developer",
                    CompanyName = "",
                    Summary = "Summary"
                }
            ]
        };

        var result = ExperienceTranslationSelector.Select(experience, "en-US");

        Assert.Null(result);
    }

    [Fact]
    public void Project_translation_and_blog_post_translation_slugs_are_unique()
    {
        using var context = CreateContext();

        var projectSlugIndex = context.Model.FindEntityType(typeof(ProjectTranslation))!
            .GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(ProjectTranslation.LanguageCode), nameof(ProjectTranslation.Slug)]));
        var blogPostSlugIndex = context.Model.FindEntityType(typeof(BlogPostTranslation))!
            .GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(BlogPostTranslation.LanguageCode), nameof(BlogPostTranslation.Slug)]));

        Assert.True(projectSlugIndex.IsUnique);
        Assert.True(blogPostSlugIndex.IsUnique);
    }

    [Fact]
    public void Blog_post_translations_have_localized_fields_and_cascade_delete()
    {
        using var context = CreateContext();

        var blogPostEntity = context.Model.FindEntityType(typeof(BlogPost))!;
        var translationEntity = context.Model.FindEntityType(typeof(BlogPostTranslation))!;

        Assert.DoesNotContain("Title", blogPostEntity.GetProperties().Select(property => property.Name));
        Assert.DoesNotContain("Slug", blogPostEntity.GetProperties().Select(property => property.Name));
        Assert.DoesNotContain("Excerpt", blogPostEntity.GetProperties().Select(property => property.Name));
        Assert.DoesNotContain("Content", blogPostEntity.GetProperties().Select(property => property.Name));
        var featuredImagePath = blogPostEntity.FindProperty(nameof(BlogPost.FeaturedImagePath))!;
        Assert.True(featuredImagePath.IsNullable);
        Assert.Equal(500, featuredImagePath.GetMaxLength());
        var featuredImageAlt = translationEntity.FindProperty(nameof(BlogPostTranslation.FeaturedImageAlt))!;
        Assert.True(featuredImageAlt.IsNullable);
        Assert.Equal(500, featuredImageAlt.GetMaxLength());
        Assert.Equal(
            [nameof(BlogPostTranslation.BlogPostId), nameof(BlogPostTranslation.LanguageCode)],
            translationEntity.FindPrimaryKey()!.Properties.Select(property => property.Name));

        var foreignKey = translationEntity.GetForeignKeys().Single();
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);

        var designTimeTranslationEntity = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(BlogPostTranslation))!;
        var languageConstraint = designTimeTranslationEntity.GetCheckConstraints()
            .Single(constraint => constraint.Name == "CK_BlogPostTranslations_LanguageCode");
        Assert.Contains("es-ES", languageConstraint.Sql);
        Assert.Contains("en-US", languageConstraint.Sql);
    }

    [Fact]
    public void Blog_post_translation_selector_uses_requested_culture_and_common_fields()
    {
        var publishedOn = new DateTimeOffset(2026, 9, 22, 12, 0, 0, TimeSpan.Zero);
        var blogPost = CreateBlogPost(
            Guid.NewGuid(),
            publishedOn,
            isFeatured: true,
            new BlogPostTranslation
            {
                LanguageCode = "es-ES",
                Title = "Artículo",
                Slug = "articulo",
                Excerpt = "Resumen",
                Content = "Contenido",
                FeaturedImageAlt = "Imagen del artículo"
            },
            new BlogPostTranslation
            {
                LanguageCode = "en-US",
                Title = "Article",
                Slug = "article",
                Excerpt = "Summary",
                Content = "Content",
                FeaturedImageAlt = "Article image"
            });
        blogPost.FeaturedImagePath = "/images/blog/article.webp";

        var result = BlogPostTranslationSelector.Select(blogPost, "en-US");

        Assert.NotNull(result);
        Assert.Equal("Article", result.Title);
        Assert.Equal("article", result.Slug);
        Assert.Equal("Summary", result.Excerpt);
        Assert.Equal("Content", result.Content);
        Assert.Equal(blogPost.FeaturedImagePath, result.FeaturedImagePath);
        Assert.Equal("Article image", result.FeaturedImageAlt);
        Assert.Equal(publishedOn, result.PublishedOn);
        Assert.True(result.IsFeatured);
    }

    [Fact]
    public void Blog_post_translation_selector_uses_title_as_image_alt_fallback()
    {
        var blogPost = CreateBlogPost(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            isFeatured: true,
            new BlogPostTranslation
            {
                LanguageCode = "es-ES",
                Title = "Artículo sin alt",
                Slug = "articulo-sin-alt",
                Excerpt = "Resumen",
                Content = "Contenido"
            });

        var result = BlogPostTranslationSelector.Select(blogPost, "es-ES");

        Assert.NotNull(result);
        Assert.Equal("Artículo sin alt", result.FeaturedImageAlt);
    }

    [Fact]
    public void Blog_post_translation_selector_falls_back_and_discards_invalid_posts()
    {
        var validPost = CreateBlogPost(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            isFeatured: false,
            new BlogPostTranslation
            {
                LanguageCode = "es-ES",
                Title = "Artículo",
                Slug = "articulo",
                Excerpt = "Resumen",
                Content = "Contenido"
            });
        var invalidPost = CreateBlogPost(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddDays(1),
            isFeatured: true,
            new BlogPostTranslation
            {
                LanguageCode = "en-US",
                Title = "Article",
                Slug = "article",
                Excerpt = "",
                Content = "Content"
            });

        var result = BlogPostTranslationSelector.SelectFeatured(
            [invalidPost, validPost],
            "en-US");

        Assert.NotNull(result);
        Assert.Equal(validPost.Id, result.Id);
        Assert.Equal("Artículo", result.Title);
    }

    [Fact]
    public void Blog_post_translation_selector_orders_featured_posts_then_date_and_id()
    {
        var date = DateTimeOffset.UtcNow;
        var earlierId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var laterId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var posts = new[]
        {
            CreateBlogPost(laterId, date, isFeatured: true, CreateTranslation("later")),
            CreateBlogPost(earlierId, date, isFeatured: true, CreateTranslation("earlier")),
            CreateBlogPost(Guid.NewGuid(), date.AddDays(1), isFeatured: false, CreateTranslation("recent"))
        };

        var result = BlogPostTranslationSelector.SelectFeatured(posts, "es-ES");

        Assert.NotNull(result);
        Assert.Equal(earlierId, result.Id);
    }

    [Fact]
    public void Project_translations_have_localized_fields_and_required_constraints()
    {
        using var context = CreateContext();

        var projectEntity = context.Model.FindEntityType(typeof(Project))!;
        var translationEntity = context.Model.FindEntityType(typeof(ProjectTranslation))!;

        var projectProperties = projectEntity.GetProperties().Select(property => property.Name);
        Assert.DoesNotContain("Title", projectProperties);
        Assert.DoesNotContain("Slug", projectProperties);
        Assert.DoesNotContain("Summary", projectProperties);
        Assert.DoesNotContain("Description", projectProperties);

        Assert.Equal(
            [nameof(ProjectTranslation.ProjectId), nameof(ProjectTranslation.LanguageCode)],
            translationEntity.FindPrimaryKey()!.Properties.Select(property => property.Name));

        var languageSlugIndex = translationEntity.GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(ProjectTranslation.LanguageCode), nameof(ProjectTranslation.Slug)]));
        Assert.True(languageSlugIndex.IsUnique);

        var foreignKey = translationEntity.GetForeignKeys().Single();
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);

        var designTimeTranslationEntity = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(ProjectTranslation))!;
        var languageConstraint = designTimeTranslationEntity.GetCheckConstraints()
            .Single(constraint => constraint.Name == "CK_ProjectTranslations_LanguageCode");
        Assert.Contains("es-ES", languageConstraint.Sql);
        Assert.Contains("en-US", languageConstraint.Sql);
    }

    [Fact]
    public void Project_preview_image_path_is_optional_and_limited()
    {
        using var context = CreateContext();

        var previewImagePath = context.Model.FindEntityType(typeof(Project))!
            .FindProperty(nameof(Project.PreviewImagePath))!;

        Assert.True(previewImagePath.IsNullable);
        Assert.Equal(500, previewImagePath.GetMaxLength());
    }

    [Fact]
    public void Certification_translations_have_required_constraints_and_cascade_delete()
    {
        using var context = CreateContext();

        var certificationEntity = context.Model.FindEntityType(typeof(Certification))!;
        var translationEntity = context.Model.FindEntityType(typeof(CertificationTranslation))!;

        Assert.DoesNotContain("Name", certificationEntity.GetProperties().Select(property => property.Name));
        Assert.DoesNotContain("Issuer", certificationEntity.GetProperties().Select(property => property.Name));
        Assert.Equal(
            [nameof(CertificationTranslation.CertificationId), nameof(CertificationTranslation.LanguageCode)],
            translationEntity.FindPrimaryKey()!.Properties.Select(property => property.Name));

        var foreignKey = translationEntity.GetForeignKeys().Single();
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);

        var designTimeTranslationEntity = context.GetService<IDesignTimeModel>().Model
            .FindEntityType(typeof(CertificationTranslation))!;
        var languageConstraint = designTimeTranslationEntity.GetCheckConstraints()
            .Single(constraint => constraint.Name == "CK_CertificationTranslations_LanguageCode");
        Assert.Contains("es-ES", languageConstraint.Sql);
        Assert.Contains("en-US", languageConstraint.Sql);
    }

    [Fact]
    public void Certification_translation_selector_uses_requested_culture_and_common_fields()
    {
        var certification = new Certification
        {
            Id = Guid.NewGuid(),
            IssuedOn = new DateOnly(2025, 4, 15),
            CredentialUrl = "https://example.com/credential",
            ImagePath = "/images/certifications/certification.webp",
            DisplayOrder = 3,
            Translations =
            [
                new CertificationTranslation
                {
                    LanguageCode = "es-ES",
                    Name = "Certificación",
                    Issuer = "Emisor"
                },
                new CertificationTranslation
                {
                    LanguageCode = "en-US",
                    Name = "Certification",
                    Issuer = "Issuer"
                }
            ]
        };

        var result = CertificationTranslationSelector.Select(certification, "en-US");

        Assert.NotNull(result);
        Assert.Equal("Certification", result.Name);
        Assert.Equal("Issuer", result.Issuer);
        Assert.Equal(certification.IssuedOn, result.IssuedOn);
        Assert.Equal(certification.CredentialUrl, result.CredentialUrl);
        Assert.Equal(certification.ImagePath, result.ImagePath);
        Assert.Equal(3, result.DisplayOrder);
    }

    [Fact]
    public void Certification_image_path_is_optional_and_limited()
    {
        using var context = CreateContext();

        var imagePath = context.Model.FindEntityType(typeof(Certification))!
            .FindProperty(nameof(Certification.ImagePath))!;

        Assert.True(imagePath.IsNullable);
        Assert.Equal(500, imagePath.GetMaxLength());
    }

    [Fact]
    public void Certification_translation_selector_falls_back_to_spanish()
    {
        var certification = new Certification
        {
            Translations =
            [
                new CertificationTranslation
                {
                    LanguageCode = "es-ES",
                    Name = "Certificación",
                    Issuer = "Emisor"
                }
            ]
        };

        var result = CertificationTranslationSelector.Select(certification, "en-US");

        Assert.NotNull(result);
        Assert.Equal("Certificación", result.Name);
    }

    [Fact]
    public void Certification_translation_selector_discards_certification_without_valid_translation()
    {
        var certification = new Certification
        {
            Translations =
            [
                new CertificationTranslation
                {
                    LanguageCode = "en-US",
                    Name = "",
                    Issuer = "Issuer"
                }
            ]
        };

        var result = CertificationTranslationSelector.Select(certification, "en-US");

        Assert.Null(result);
    }

    [Fact]
    public void Project_translation_selector_uses_requested_culture_and_common_fields()
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            RepositoryUrl = "https://github.com/example/project",
            DemoUrl = "https://example.com/project",
            PreviewImagePath = "/images/projects/project.webp",
            IsFeatured = true,
            DisplayOrder = 2,
            Translations =
            [
                new ProjectTranslation
                {
                    LanguageCode = "es-ES",
                    Title = "Proyecto",
                    Slug = "proyecto",
                    Summary = "Resumen en español"
                },
                new ProjectTranslation
                {
                    LanguageCode = "en-US",
                    Title = "Project",
                    Slug = "project",
                    Summary = "English summary"
                }
            ]
        };

        var result = ProjectTranslationSelector.Select(project, "en-US");

        Assert.NotNull(result);
        Assert.Equal("Project", result.Title);
        Assert.Equal("project", result.Slug);
        Assert.Equal("English summary", result.Summary);
        Assert.Equal(project.PreviewImagePath, result.PreviewImagePath);
        Assert.Equal(project.RepositoryUrl, result.RepositoryUrl);
        Assert.Equal(project.DemoUrl, result.DemoUrl);
        Assert.True(result.IsFeatured);
        Assert.Equal(2, result.DisplayOrder);
    }

    [Fact]
    public void Project_translation_selector_falls_back_to_spanish()
    {
        var project = new Project
        {
            Translations =
            [
                new ProjectTranslation
                {
                    LanguageCode = "es-ES",
                    Title = "Proyecto",
                    Slug = "proyecto",
                    Summary = "Resumen"
                }
            ]
        };

        var result = ProjectTranslationSelector.Select(project, "en-US");

        Assert.NotNull(result);
        Assert.Equal("Proyecto", result.Title);
    }

    [Fact]
    public void Project_translation_selector_discards_project_without_valid_translation()
    {
        var project = new Project
        {
            Translations =
            [
                new ProjectTranslation
                {
                    LanguageCode = "en-US",
                    Title = "",
                    Slug = "project",
                    Summary = "Summary"
                }
            ]
        };

        var result = ProjectTranslationSelector.Select(project, "en-US");

        Assert.Null(result);
    }

    private static BlogPost CreateBlogPost(
        Guid id,
        DateTimeOffset publishedOn,
        bool isFeatured,
        params BlogPostTranslation[] translations)
    {
        return new BlogPost
        {
            Id = id,
            PublishedOn = publishedOn,
            IsPublished = true,
            IsFeatured = isFeatured,
            Translations = translations
        };
    }

    private static BlogPostTranslation CreateTranslation(string slug)
    {
        return new BlogPostTranslation
        {
            LanguageCode = "es-ES",
            Title = slug,
            Slug = slug,
            Excerpt = "Resumen",
            Content = "Contenido"
        };
    }

    private static PortfolioDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PortfolioModelTests;Trusted_Connection=True;")
            .Options;

        return new PortfolioDbContext(options);
    }
}
