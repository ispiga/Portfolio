using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
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
            ["BlogPost", "Certification", "Experience", "Project", "ProjectTranslation"],
            entityNames);
    }

    [Fact]
    public void Project_translation_and_blog_post_slugs_are_unique()
    {
        using var context = CreateContext();

        var projectSlugIndex = context.Model.FindEntityType(typeof(ProjectTranslation))!
            .GetIndexes()
            .Single(index => index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(ProjectTranslation.LanguageCode), nameof(ProjectTranslation.Slug)]));
        var blogPostSlugIndex = context.Model.FindEntityType(typeof(BlogPost))!
            .GetIndexes()
            .Single(index => index.Properties.Single().Name == nameof(BlogPost.Slug));

        Assert.True(projectSlugIndex.IsUnique);
        Assert.True(blogPostSlugIndex.IsUnique);
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

    private static PortfolioDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PortfolioModelTests;Trusted_Connection=True;")
            .Options;

        return new PortfolioDbContext(options);
    }
}
