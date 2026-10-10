using Microsoft.EntityFrameworkCore;
using Portfolio.Application.HomeContent;

namespace Portfolio.Infrastructure.Services;

public sealed class HeroQueryService(IDbContextFactory<PortfolioDbContext> dbContextFactory) : IHeroQueryService
{
    public async Task<HeroReadModel?> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var content = await context.HeroContents.AsNoTracking()
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken);
        if (content is null)
        {
            return null;
        }

        var languageCode = HomeContentTranslationSelector.CurrentLanguageCode;
        var headline = HomeContentTranslationSelector.Select(
            content.Translations.SingleOrDefault(item => item.LanguageCode == languageCode)?.Headline,
            content.Translations.SingleOrDefault(item => item.LanguageCode == "es-ES")?.Headline);
        if (string.IsNullOrWhiteSpace(headline))
        {
            return null;
        }

        var logos = await context.TechnologyLogos.AsNoTracking()
            .OrderBy(logo => logo.Orbit)
            .ThenBy(logo => logo.DisplayOrder)
            .ThenBy(logo => logo.Id)
            .Select(logo => new TechnologyLogoReadModel(
                logo.Id,
                logo.Name,
                logo.AlternativeText,
                $"/home-content-images/technology-logo/{logo.Id:N}",
                logo.DisplayOrder,
                logo.Orbit))
            .ToArrayAsync(cancellationToken);

        return new HeroReadModel(
            headline,
            content.OrbitCount,
            logos,
            content.ProfileImageId is { } profileImageId
                ? $"/home-content-images/hero-profile/{profileImageId:N}"
                : null,
            content.ProfileImageAlternativeText);
    }
}