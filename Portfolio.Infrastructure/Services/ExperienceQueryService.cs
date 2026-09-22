using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Experiences;

namespace Portfolio.Infrastructure.Services;

public sealed class ExperienceQueryService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory) : IExperienceQueryService
{
    public async Task<IReadOnlyList<ExperienceReadModel>> GetExperiencesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var experiences = await context.Experiences
            .AsNoTracking()
            .Include(experience => experience.Translations)
            .OrderBy(experience => experience.DisplayOrder)
            .ThenBy(experience => experience.Id)
            .ToListAsync(cancellationToken);

        return experiences
            .Select(experience => ExperienceTranslationSelector.Select(
                experience,
                CultureInfo.CurrentUICulture.Name))
            .OfType<ExperienceReadModel>()
            .ToArray();
    }
}