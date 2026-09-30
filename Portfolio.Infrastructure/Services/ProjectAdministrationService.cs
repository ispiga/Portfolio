using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Projects;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Services;

public sealed class ProjectAdministrationService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory) : IProjectAdministrationService
{
    public async Task<IReadOnlyList<ProjectAdminListItem>> GetProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var projects = await context.Projects
            .AsNoTracking()
            .Include(project => project.Translations)
            .OrderBy(project => project.DisplayOrder)
            .ThenBy(project => project.Id)
            .ToListAsync(cancellationToken);

        return projects.Select(project =>
        {
            var spanish = FindTranslation(project, "es-ES");
            return new ProjectAdminListItem(
                project.Id,
                spanish?.Title ?? string.Empty,
                project.DisplayOrder,
                project.IsFeatured,
                IsComplete(spanish),
                IsComplete(FindTranslation(project, "en-US")),
                !string.IsNullOrWhiteSpace(project.PreviewImagePath));
        }).ToArray();
    }

    public async Task<ProjectAdminDetails?> GetProjectAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var project = await context.Projects
            .AsNoTracking()
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        return project is null ? null : ToDetails(project);
    }

    public async Task<ProjectSaveResult> SaveAsync(
        ProjectEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ProjectEditValidator.Validate(request).ToList();
        if (errors.Count > 0)
        {
            return new(false, null, errors);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        foreach (var (languageCode, translation) in new[]
        {
            ("es-ES", request.Spanish),
            ("en-US", request.English)
        })
        {
            if (string.IsNullOrWhiteSpace(translation.Slug))
            {
                continue;
            }

            var duplicateSlug = await context.ProjectTranslations.AnyAsync(candidate =>
                candidate.LanguageCode == languageCode
                && candidate.Slug == translation.Slug.Trim()
                && candidate.ProjectId != request.Id,
                cancellationToken);
            if (duplicateSlug)
            {
                errors.Add(new($"{(languageCode == "es-ES" ? "Spanish" : "English")}.Slug", "ProjectSlugDuplicate"));
            }
        }

        if (errors.Count > 0)
        {
            return new(false, null, errors);
        }

        Project project;
        if (request.Id is { } id)
        {
            var existing = await context.Projects
                .Include(candidate => candidate.Translations)
                .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            if (existing is null)
            {
                return new(false, null, [new(nameof(request.Id), "ProjectNotFound")]);
            }

            project = existing;
        }
        else
        {
            project = new Project { Id = Guid.NewGuid() };
            context.Projects.Add(project);
        }

        project.RepositoryUrl = NormalizeOptional(request.RepositoryUrl);
        project.DemoUrl = NormalizeOptional(request.DemoUrl);
        project.IsFeatured = request.IsFeatured;
        project.DisplayOrder = request.DisplayOrder;
        UpdateTranslation(project, "es-ES", request.Spanish);
        UpdateTranslation(project, "en-US", request.English);

        await context.SaveChangesAsync(cancellationToken);

        return new(true, project.Id, []);
    }

    public async Task<ProjectDeleteResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var project = await context.Projects
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (project is null)
        {
            return ProjectDeleteResult.NotFound;
        }

        context.ProjectTranslations.RemoveRange(project.Translations);
        context.Projects.Remove(project);
        await context.SaveChangesAsync(cancellationToken);
        return ProjectDeleteResult.Deleted;
    }

    private static ProjectAdminDetails ToDetails(Project project)
    {
        var spanish = FindTranslation(project, "es-ES");
        var english = FindTranslation(project, "en-US");
        return new ProjectAdminDetails(
            project.Id,
            project.RepositoryUrl,
            project.DemoUrl,
            project.PreviewImagePath,
            project.IsFeatured,
            project.DisplayOrder,
            ToInput(spanish),
            ToInput(english),
            IsComplete(spanish),
            IsComplete(english));
    }

    private static ProjectTranslation? FindTranslation(Project project, string languageCode) =>
        project.Translations.SingleOrDefault(translation => translation.LanguageCode == languageCode);

    private static bool IsComplete(ProjectTranslation? translation) =>
        translation is not null
        && !string.IsNullOrWhiteSpace(translation.Title)
        && !string.IsNullOrWhiteSpace(translation.Summary);

    private static ProjectTranslationInput ToInput(ProjectTranslation? translation) => new(
        translation?.Title ?? string.Empty,
        translation?.Slug,
        translation?.Summary ?? string.Empty,
        translation?.Description);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void UpdateTranslation(Project project, string languageCode, ProjectTranslationInput input)
    {
        var translation = FindTranslation(project, languageCode);
        if (string.IsNullOrWhiteSpace(input.Title)
            && string.IsNullOrWhiteSpace(input.Slug)
            && string.IsNullOrWhiteSpace(input.Summary)
            && string.IsNullOrWhiteSpace(input.Description))
        {
            if (translation is not null)
            {
                project.Translations.Remove(translation);
            }

            return;
        }

        if (translation is null)
        {
            translation = new ProjectTranslation { LanguageCode = languageCode };
            project.Translations.Add(translation);
        }

        translation.Title = input.Title.Trim();
        if (!string.IsNullOrWhiteSpace(input.Slug))
        {
            translation.Slug = input.Slug.Trim();
        }
        translation.Summary = input.Summary.Trim();
        translation.Description = NormalizeOptional(input.Description);
    }
}
