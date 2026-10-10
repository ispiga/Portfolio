using Microsoft.EntityFrameworkCore;
using Portfolio.Application.HomeContent;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Services;

public sealed class HeroContentAdministrationService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory,
    IHomeContentImageStorageService imageStorage) : IHeroAdministrationService
{
    public async Task<HeroEditRequest> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var content = await context.HeroContents.AsNoTracking()
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken);
        var logos = await context.TechnologyLogos.AsNoTracking()
            .OrderBy(logo => logo.DisplayOrder)
            .ThenBy(logo => logo.Id)
            .ToArrayAsync(cancellationToken);
        return new HeroEditRequest(
            content?.OrbitCount ?? 1,
            content?.Translations.SingleOrDefault(translation => translation.LanguageCode == "es-ES")?.Headline ?? string.Empty,
            content?.Translations.SingleOrDefault(translation => translation.LanguageCode == "en-US")?.Headline,
            logos.Select(logo => new TechnologyLogoEditModel(
                logo.Id, logo.Name, logo.AlternativeText, logo.StorageKey, logo.ContentType,
                logo.SizeBytes, logo.DisplayOrder, logo.Orbit)).ToArray(),
            content?.ProfileImageId,
            content?.ProfileImageStorageKey,
            content?.ProfileImageContentType,
            content?.ProfileImageSizeBytes,
            content?.ProfileImageAlternativeText);
    }

    public async Task<HeroSaveResult> SaveAsync(
        HeroEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = HeroContentValidator.Validate(request);
        if (errors.Count > 0)
        {
            return new(false, errors);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var content = await context.HeroContents
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken);
        var previousProfileImageId = content?.ProfileImageId;
        var previousProfileImageKey = content?.ProfileImageStorageKey;
        if (content is null)
        {
            content = new HeroContent { Id = 1 };
            context.HeroContents.Add(content);
        }

        content.OrbitCount = request.OrbitCount;
        content.ProfileImageId = request.ProfileImageId;
        content.ProfileImageStorageKey = request.ProfileImageStorageKey;
        content.ProfileImageContentType = request.ProfileImageContentType;
        content.ProfileImageSizeBytes = request.ProfileImageSizeBytes;
        content.ProfileImageAlternativeText = request.ProfileImageAlternativeText?.Trim();
        SetHeadline(content, "es-ES", request.SpanishHeadline);
        SetHeadline(content, "en-US", request.EnglishHeadline);

        var existing = await context.TechnologyLogos.ToDictionaryAsync(logo => logo.Id, cancellationToken);
        var requestedIds = request.Logos.Select(logo => logo.Id).ToHashSet();
        var removedImages = existing.Values
            .Where(logo => !requestedIds.Contains(logo.Id)
                || !string.Equals(logo.StorageKey, request.Logos.Single(item => item.Id == logo.Id).StorageKey, StringComparison.Ordinal))
            .Select(logo => (logo.Id, logo.StorageKey))
            .ToArray();

        foreach (var removed in existing.Values.Where(logo => !requestedIds.Contains(logo.Id)))
        {
            context.TechnologyLogos.Remove(removed);
        }

        foreach (var item in request.Logos)
        {
            if (!existing.TryGetValue(item.Id, out var logo))
            {
                logo = new TechnologyLogo { Id = item.Id };
                context.TechnologyLogos.Add(logo);
            }

            logo.Name = item.Name.Trim();
            logo.AlternativeText = item.AlternativeText.Trim();
            logo.StorageKey = item.StorageKey;
            logo.ContentType = item.ContentType;
            logo.SizeBytes = item.SizeBytes;
            logo.DisplayOrder = item.DisplayOrder;
            logo.Orbit = item.Orbit;
        }

        await context.SaveChangesAsync(cancellationToken);
        foreach (var (id, key) in removedImages)
        {
            await imageStorage.DeleteAsync(HomeContentImageKind.TechnologyLogo, id, key);
        }

        if (previousProfileImageId is { } previousImageId
            && (previousImageId != request.ProfileImageId
                || !string.Equals(previousProfileImageKey, request.ProfileImageStorageKey, StringComparison.Ordinal)))
        {
            await imageStorage.DeleteAsync(HomeContentImageKind.HeroProfile, previousImageId, previousProfileImageKey);
        }

        return new(true, []);
    }

    private static void SetHeadline(HeroContent content, string languageCode, string? value)
    {
        var translation = content.Translations.SingleOrDefault(item => item.LanguageCode == languageCode);
        if (string.IsNullOrWhiteSpace(value))
        {
            if (translation is not null)
            {
                content.Translations.Remove(translation);
            }

            return;
        }

        if (translation is null)
        {
            content.Translations.Add(new HeroTranslation
            {
                HeroContentId = content.Id,
                LanguageCode = languageCode,
                Headline = value.Trim()
            });
        }
        else
        {
            translation.Headline = value.Trim();
        }
    }
}