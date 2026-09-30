using System.Globalization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Projects;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Services;
using Xunit;

namespace Portfolio.Tests;

public sealed class ProjectAdministrationTests
{
    [Fact]
    public async Task Create_and_edit_persist_localizations_order_featured_and_urls()
    {
        var factory = CreateFactory();
        var service = new ProjectAdministrationService(factory);
        var created = await service.SaveAsync(ValidRequest() with { DisplayOrder = 4, IsFeatured = true });

        Assert.True(created.Succeeded);
        var details = await service.GetProjectAsync(created.ProjectId!.Value);
        Assert.NotNull(details);
        Assert.True(details.HasSpanishTranslation);
        Assert.True(details.HasEnglishTranslation);
        Assert.Equal("Portfolio personal", details.Spanish.Title);
        Assert.Equal("portfolio-personal", details.Spanish.Slug);
        Assert.Equal("Personal portfolio", details.English.Title);
        Assert.Equal("personal-portfolio", details.English.Slug);
        Assert.Equal(4, details.DisplayOrder);
        Assert.True(details.IsFeatured);
        Assert.Equal("https://example.test/repository", details.RepositoryUrl);

        var updated = await service.SaveAsync(ValidRequest() with
        {
            Id = created.ProjectId,
            DisplayOrder = 1,
            IsFeatured = false,
            English = EmptyTranslation()
        });

        Assert.True(updated.Succeeded);
        details = await service.GetProjectAsync(created.ProjectId.Value);
        Assert.NotNull(details);
        Assert.Equal(1, details.DisplayOrder);
        Assert.False(details.IsFeatured);
        Assert.False(details.HasEnglishTranslation);
    }

    [Fact]
    public async Task Missing_english_translation_is_allowed_and_public_query_falls_back_to_spanish()
    {
        var factory = CreateFactory();
        var result = await new ProjectAdministrationService(factory).SaveAsync(
            ValidRequest() with { English = EmptyTranslation() });
        Assert.True(result.Succeeded);
        var storedPreviewPath = "/project-preview-images/first-preview-file.jpg";

        await using (var context = factory.CreateDbContext())
        {
            var project = await context.Projects.SingleAsync(candidate => candidate.Id == result.ProjectId);
            project.PreviewImagePath = storedPreviewPath;
            await context.SaveChangesAsync();
        }

        var previousCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var publicProject = Assert.Single(await new ProjectQueryService(factory).GetProjectsAsync());
            Assert.Equal("Portfolio personal", publicProject.Title);
            Assert.Equal($"/project-preview-images/{result.ProjectId}?v=first-preview-file.jpg", publicProject.PreviewImagePath);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public async Task Admin_and_public_project_queries_preserve_display_order()
    {
        var factory = CreateFactory();
        var service = new ProjectAdministrationService(factory);
        var later = await service.SaveAsync(ValidRequest() with { DisplayOrder = 5, IsFeatured = true });
        var first = await service.SaveAsync(ValidRequest() with
        {
            Spanish = new("Otro proyecto", "otro-proyecto", "Resumen del otro proyecto", null),
            English = new("Other project", "other-project", "Other summary", null),
            DisplayOrder = 1
        });

        var adminItems = await service.GetProjectsAsync();
        Assert.Equal(new[] { first.ProjectId, later.ProjectId }, adminItems.Select(item => (Guid?)item.Id));
        var publicItems = await new ProjectQueryService(factory).GetProjectsAsync();
        Assert.Equal(new[] { first.ProjectId, later.ProjectId }, publicItems.Select(item => (Guid?)item.Id));
    }

    [Fact]
    public async Task Invalid_order_partial_translation_and_urls_are_rejected()
    {
        var factory = CreateFactory();
        var service = new ProjectAdministrationService(factory);
        var invalid = await service.SaveAsync(ValidRequest() with
        {
            DisplayOrder = -1,
            RepositoryUrl = "javascript:alert(1)",
            English = new("Developer", "developer", string.Empty, null)
        });

        Assert.False(invalid.Succeeded);
        Assert.Contains(invalid.Errors, error => error.ResourceKey == "ProjectDisplayOrderInvalid");
        Assert.Contains(invalid.Errors, error => error.ResourceKey == "ProjectUrlInvalid");
        Assert.Contains(invalid.Errors, error => error.Field == "English.Summary");
    }

    [Fact]
    public async Task Duplicate_slug_is_rejected_within_the_same_language()
    {
        var service = new ProjectAdministrationService(CreateFactory());
        var first = await service.SaveAsync(ValidRequest());
        var duplicate = await service.SaveAsync(ValidRequest() with
        {
            Spanish = new("Otro proyecto", "portfolio-personal", "Resumen", null),
            English = EmptyTranslation()
        });

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Contains(duplicate.Errors, error =>
            error.Field == "Spanish.Slug" && error.ResourceKey == "ProjectSlugDuplicate");
    }

    [Fact]
    public async Task Project_can_be_saved_without_slug_and_existing_slug_is_preserved()
    {
        var factory = CreateFactory();
        var service = new ProjectAdministrationService(factory);
        var created = await service.SaveAsync(ValidRequest() with
        {
            Spanish = new("Portfolio personal", null, "Resumen del portfolio", "Descripción del proyecto"),
            English = EmptyTranslation()
        });

        Assert.True(created.Succeeded);
        var details = await service.GetProjectAsync(created.ProjectId!.Value);
        Assert.NotNull(details);
        Assert.Null(details.Spanish.Slug);

        await using (var context = factory.CreateDbContext())
        {
            var translation = await context.ProjectTranslations.SingleAsync(item => item.ProjectId == created.ProjectId);
            translation.Slug = "slug-anterior";
            await context.SaveChangesAsync();
        }

        var updated = await service.SaveAsync(ValidRequest() with
        {
            Id = created.ProjectId,
            Spanish = new("Portfolio personal", null, "Resumen del portfolio", "Descripción del proyecto"),
            English = EmptyTranslation()
        });
        Assert.True(updated.Succeeded);
        details = await service.GetProjectAsync(created.ProjectId.Value);
        Assert.NotNull(details);
        Assert.Equal("slug-anterior", details.Spanish.Slug);
    }

    [Fact]
    public async Task Delete_removes_project_and_translations()
    {
        var factory = CreateFactory();
        var service = new ProjectAdministrationService(factory);
        var result = await service.SaveAsync(ValidRequest());

        Assert.Equal(ProjectDeleteResult.Deleted, await service.DeleteAsync(result.ProjectId!.Value));
        Assert.Null(await service.GetProjectAsync(result.ProjectId.Value));
        Assert.Equal(ProjectDeleteResult.NotFound, await service.DeleteAsync(result.ProjectId.Value));
        await using var context = factory.CreateDbContext();
        Assert.Empty(await context.ProjectTranslations.ToListAsync());
    }

    [Fact]
    public void Project_editor_and_list_require_administrator_policy()
    {
        var editorSource = File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "Portfolio.Web", "Components", "Admin", "ProjectEditor.razor"));

        Assert.Contains("id=\"project-slug-es\" value=", editorSource, StringComparison.Ordinal);
        Assert.Contains("id=\"project-slug-en\" value=", editorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("InputText id=\"project-slug-es\"", editorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("InputText id=\"project-slug-en\"", editorSource, StringComparison.Ordinal);
        Assert.Contains("data-ignore-unsaved-changes", editorSource, StringComparison.Ordinal);
        Assert.Contains("?v={previewImageVersion}", editorSource, StringComparison.Ordinal);

        Assert.Equal(
            Portfolio.Infrastructure.Identity.PortfolioAuthorization.AdministratorPolicy,
            typeof(Portfolio.Web.Components.Admin.Projects).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Policy);
        Assert.Equal(
            Portfolio.Infrastructure.Identity.PortfolioAuthorization.AdministratorPolicy,
            typeof(Portfolio.Web.Components.Admin.ProjectEditor).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Policy);
    }

    private static ProjectEditRequest ValidRequest() => new(
        null,
        "https://example.test/repository",
        "https://example.test/demo",
        false,
        0,
        new("Portfolio personal", "portfolio-personal", "Resumen del portfolio", "Descripción del proyecto"),
        new("Personal portfolio", "personal-portfolio", "Portfolio summary", "Project description"));

    private static ProjectTranslationInput EmptyTranslation() => new(string.Empty, string.Empty, string.Empty, null);

    private static TestContextFactory CreateFactory() => new(Guid.NewGuid().ToString());

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }
}
