using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Projects;

namespace Portfolio.Infrastructure.Services;

public sealed class ProjectQueryService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory) : IProjectQueryService
{
    public async Task<IReadOnlyList<ProjectReadModel>> GetProjectsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var projects = await context.Projects
            .AsNoTracking()
            .Include(project => project.Translations)
            .OrderBy(project => project.DisplayOrder)
            .ThenBy(project => project.Id)
            .ToListAsync(cancellationToken);

        return projects
            .Select(project => ProjectTranslationSelector.Select(
                project,
                CultureInfo.CurrentUICulture.Name))
            .OfType<ProjectReadModel>()
            .ToArray();
    }
}
