using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.VisualIdentity;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Infrastructure.Services;

public sealed class VisualIdentitySettingsService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory,
    IVisualIdentityAssetStorageService assetStorage) : IVisualIdentityAdministrationService, IVisualIdentityQueryService
{
    public async Task<VisualIdentitySettingsModel> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await context.VisualIdentitySettings.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken)
            ?? CreateDefaultSettings();
        return ToModel(settings);
    }

    public async Task<VisualIdentitySaveResult> SaveAsync(
        VisualIdentityEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = VisualIdentityValidator.Validate(request);
        if (errors.Count > 0)
        {
            return new(false, errors);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await context.VisualIdentitySettings.SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken);
        if (settings is null)
        {
            settings = CreateDefaultSettings();
            context.VisualIdentitySettings.Add(settings);
        }

        var previousLogoColors = GetEffectiveLogoColors(settings);
        var primaryColor = VisualIdentityValidator.NormalizeHexColor(request.PrimaryColor);
        var secondaryColor = VisualIdentityValidator.NormalizeHexColor(request.SecondaryColor);
        var accentColor = VisualIdentityValidator.NormalizeHexColor(request.AccentColor);
        var startOverride = request.LogoColorsLinkedToPalette
            ? null
            : VisualIdentityValidator.NormalizeHexColor(request.LogoColorStartOverride!);
        var endOverride = request.LogoColorsLinkedToPalette
            ? null
            : VisualIdentityValidator.NormalizeHexColor(request.LogoColorEndOverride!);

        var changed = !string.Equals(settings.PrimaryColor, primaryColor, StringComparison.Ordinal)
            || !string.Equals(settings.SecondaryColor, secondaryColor, StringComparison.Ordinal)
            || !string.Equals(settings.AccentColor, accentColor, StringComparison.Ordinal)
            || settings.LogoColorsLinkedToPalette != request.LogoColorsLinkedToPalette
            || !string.Equals(settings.LogoColorStartOverride, startOverride, StringComparison.Ordinal)
            || !string.Equals(settings.LogoColorEndOverride, endOverride, StringComparison.Ordinal);
        if (!changed)
        {
            return new(true, []);
        }

        settings.PrimaryColor = primaryColor;
        settings.SecondaryColor = secondaryColor;
        settings.AccentColor = accentColor;
        settings.LogoColorsLinkedToPalette = request.LogoColorsLinkedToPalette;
        settings.LogoColorStartOverride = startOverride;
        settings.LogoColorEndOverride = endOverride;

        var newLogoColors = GetEffectiveLogoColors(settings);
        if (previousLogoColors != newLogoColors)
        {
            settings.FooterLogoNeedsReview |= settings.FooterLogoStorageKey is not null;
            settings.FaviconNeedsReview |= settings.FaviconStorageKey is not null;
        }

        settings.AssetVersion = Guid.NewGuid();
        await context.SaveChangesAsync(cancellationToken);
        return new(true, []);
    }

    public async Task<VisualIdentityAssetUploadResult> UploadAssetAsync(
        VisualIdentityAssetKind kind,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var upload = await assetStorage.UploadAsync(kind, fileName, contentType, fileSize, content, cancellationToken);
        if (!upload.Succeeded || upload.StorageKey is null)
        {
            return upload;
        }

        string? previousKey = null;
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var settings = await context.VisualIdentitySettings.SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken);
            if (settings is null)
            {
                settings = CreateDefaultSettings();
                context.VisualIdentitySettings.Add(settings);
            }

            switch (kind)
            {
                case VisualIdentityAssetKind.MainLogo:
                    previousKey = settings.MainLogoStorageKey;
                    settings.MainLogoStorageKey = upload.StorageKey;
                    settings.MainLogoSizeBytes = upload.SizeBytes;
                    settings.MainLogoSupportsRecoloring = upload.SupportsAutomaticRecoloring;
                    settings.FooterLogoNeedsReview = settings.FooterLogoStorageKey is not null;
                    settings.FaviconNeedsReview = settings.FaviconStorageKey is not null;
                    break;
                case VisualIdentityAssetKind.FooterLogo:
                    previousKey = settings.FooterLogoStorageKey;
                    settings.FooterLogoStorageKey = upload.StorageKey;
                    settings.FooterLogoNeedsReview = false;
                    break;
                case VisualIdentityAssetKind.Favicon:
                    previousKey = settings.FaviconStorageKey;
                    settings.FaviconStorageKey = upload.StorageKey;
                    settings.FaviconNeedsReview = false;
                    break;
                default:
                    await assetStorage.DeleteAsync(upload.StorageKey);
                    return new(VisualIdentityAssetError.UnsupportedType);
            }

            settings.AssetVersion = Guid.NewGuid();
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await assetStorage.DeleteAsync(upload.StorageKey);
            throw;
        }

        if (!string.Equals(previousKey, upload.StorageKey, StringComparison.Ordinal))
        {
            await assetStorage.DeleteAsync(previousKey);
        }

        return upload;
    }

    public async Task RemoveManualVariantAsync(
        VisualIdentityAssetKind kind,
        CancellationToken cancellationToken = default)
    {
        string? previousKey = null;
        await using (var context = await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            var settings = await context.VisualIdentitySettings.SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken);
            if (settings is null)
            {
                return;
            }

            switch (kind)
            {
                case VisualIdentityAssetKind.MainLogo:
                    previousKey = settings.MainLogoStorageKey;
                    settings.MainLogoStorageKey = null;
                    settings.MainLogoSizeBytes = null;
                    settings.MainLogoSupportsRecoloring = true;
                    settings.FooterLogoNeedsReview = settings.FooterLogoStorageKey is not null;
                    settings.FaviconNeedsReview = settings.FaviconStorageKey is not null;
                    break;
                case VisualIdentityAssetKind.FooterLogo:
                    previousKey = settings.FooterLogoStorageKey;
                    settings.FooterLogoStorageKey = null;
                    settings.FooterLogoNeedsReview = false;
                    break;
                case VisualIdentityAssetKind.Favicon:
                    previousKey = settings.FaviconStorageKey;
                    settings.FaviconStorageKey = null;
                    settings.FaviconNeedsReview = false;
                    break;
                default:
                    return;
            }

            settings.AssetVersion = Guid.NewGuid();
            await context.SaveChangesAsync(cancellationToken);
        }

        await assetStorage.DeleteAsync(previousKey);
    }

    public async Task<VisualIdentitySaveResult> RestoreDefaultsAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await context.VisualIdentitySettings.SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken);
        if (settings is null)
        {
            settings = CreateDefaultSettings();
            context.VisualIdentitySettings.Add(settings);
            await context.SaveChangesAsync(cancellationToken);
            return new(true, []);
        }

        var previousKeys = new[]
        {
            settings.MainLogoStorageKey,
            settings.FooterLogoStorageKey,
            settings.FaviconStorageKey
        };
        settings.PrimaryColor = VisualIdentityValidator.DefaultPrimaryColor;
        settings.SecondaryColor = VisualIdentityValidator.DefaultSecondaryColor;
        settings.AccentColor = VisualIdentityValidator.DefaultAccentColor;
        settings.LogoColorsLinkedToPalette = true;
        settings.LogoColorStartOverride = null;
        settings.LogoColorEndOverride = null;
        settings.MainLogoStorageKey = null;
        settings.MainLogoSizeBytes = null;
        settings.MainLogoSupportsRecoloring = true;
        settings.FooterLogoStorageKey = null;
        settings.FaviconStorageKey = null;
        settings.FooterLogoNeedsReview = false;
        settings.FaviconNeedsReview = false;
        settings.AssetVersion = Guid.NewGuid();
        await context.SaveChangesAsync(cancellationToken);

        foreach (var key in previousKeys.Distinct(StringComparer.Ordinal))
        {
            await assetStorage.DeleteAsync(key);
        }

        return new(true, []);
    }

    public async Task<VisualIdentityAssetContent?> OpenAssetAsync(
        VisualIdentityAssetKind kind,
        Guid assetVersion,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await context.VisualIdentitySettings.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken)
            ?? CreateDefaultSettings();
        if (settings.AssetVersion != assetVersion)
        {
            return null;
        }

        if (kind == VisualIdentityAssetKind.MainLogo)
        {
            var source = await OpenMainLogoAsync(settings, cancellationToken);
            var (startColor, endColor) = GetEffectiveLogoColors(settings);
            return await TransformMainLogoAsync(source, settings, startColor, endColor, cancellationToken);
        }

        if (kind == VisualIdentityAssetKind.FooterLogo)
        {
            if (settings.FooterLogoStorageKey is not null)
            {
                return await assetStorage.OpenReadAsync(settings.FooterLogoStorageKey, cancellationToken);
            }

            if (!settings.MainLogoSupportsRecoloring)
            {
                return null;
            }

            return await CreateVariantAsync(settings, VisualIdentityAssetKind.FooterLogo, settings.PrimaryColor, cancellationToken);
        }

        if (kind == VisualIdentityAssetKind.Favicon)
        {
            if (settings.FaviconStorageKey is not null)
            {
                return await assetStorage.OpenReadAsync(settings.FaviconStorageKey, cancellationToken);
            }

            if (!settings.MainLogoSupportsRecoloring)
            {
                return await assetStorage.OpenDefaultFaviconAsync(cancellationToken);
            }

            return await CreateVariantAsync(settings, VisualIdentityAssetKind.Favicon, settings.PrimaryColor, cancellationToken)
                ?? await assetStorage.OpenDefaultFaviconAsync(cancellationToken);
        }

        return null;
    }

    public async Task<VisualIdentityAssetContent?> OpenPreviewAssetAsync(
        VisualIdentityAssetKind kind,
        VisualIdentityEditRequest request,
        CancellationToken cancellationToken = default)
    {
        if (VisualIdentityValidator.Validate(request).Count > 0)
        {
            return null;
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var settings = await context.VisualIdentitySettings.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken)
            ?? CreateDefaultSettings();

        if (kind == VisualIdentityAssetKind.MainLogo)
        {
            return await TransformMainLogoAsync(
                await OpenMainLogoAsync(settings, cancellationToken),
                settings,
                request.LogoColorsLinkedToPalette
                    ? VisualIdentityValidator.NormalizeHexColor(request.PrimaryColor)
                    : VisualIdentityValidator.NormalizeHexColor(request.LogoColorStartOverride!),
                request.LogoColorsLinkedToPalette
                    ? VisualIdentityValidator.NormalizeHexColor(request.SecondaryColor)
                    : VisualIdentityValidator.NormalizeHexColor(request.LogoColorEndOverride!),
                cancellationToken);
        }

        if (kind == VisualIdentityAssetKind.FooterLogo && settings.FooterLogoStorageKey is not null)
        {
            return await assetStorage.OpenReadAsync(settings.FooterLogoStorageKey, cancellationToken);
        }

        if (kind == VisualIdentityAssetKind.Favicon && settings.FaviconStorageKey is not null)
        {
            return await assetStorage.OpenReadAsync(settings.FaviconStorageKey, cancellationToken);
        }

        if (!settings.MainLogoSupportsRecoloring)
        {
            return kind == VisualIdentityAssetKind.Favicon
                ? await assetStorage.OpenDefaultFaviconAsync(cancellationToken)
                : null;
        }

        return await CreateVariantAsync(
            settings,
            kind,
            VisualIdentityValidator.NormalizeHexColor(request.PrimaryColor),
            cancellationToken);
    }

    private async Task<VisualIdentityAssetContent> OpenMainLogoAsync(
        VisualIdentitySettings settings,
        CancellationToken cancellationToken) => settings.MainLogoStorageKey is null
        ? await assetStorage.OpenDefaultLogoAsync(cancellationToken)
        : await assetStorage.OpenReadAsync(settings.MainLogoStorageKey, cancellationToken)
            ?? await assetStorage.OpenDefaultLogoAsync(cancellationToken);

    private async Task<VisualIdentityAssetContent?> TransformMainLogoAsync(
        VisualIdentityAssetContent? source,
        VisualIdentitySettings settings,
        string startColor,
        string endColor,
        CancellationToken cancellationToken)
    {
        if (source is null || source.ContentType != "image/svg+xml" || !settings.MainLogoSupportsRecoloring)
        {
            return source;
        }

        await using (source.Content)
        {
            var document = await SafeSvgDocument.LoadValidatedAsync(source.Content, cancellationToken);
            var transformed = document is null
                ? null
                : VisualIdentitySvgTransformer.ApplyBrandColors(document, startColor, endColor);
            return transformed is null ? null : ToContent(transformed);
        }
    }

    private async Task<VisualIdentityAssetContent?> CreateVariantAsync(
        VisualIdentitySettings settings,
        VisualIdentityAssetKind kind,
        string faviconColor,
        CancellationToken cancellationToken)
    {
        var source = await OpenMainLogoAsync(settings, cancellationToken);
        await using (source.Content)
        {
            var document = await SafeSvgDocument.LoadValidatedAsync(source.Content, cancellationToken);
            if (document is null)
            {
                return null;
            }

            var variant = kind switch
            {
                VisualIdentityAssetKind.FooterLogo => VisualIdentitySvgTransformer.CreateWhiteVariant(document),
                VisualIdentityAssetKind.Favicon => VisualIdentitySvgTransformer.CreateFaviconVariant(document, faviconColor),
                _ => null
            };
            return variant is null ? null : ToContent(variant);
        }
    }

    private VisualIdentitySettingsModel ToModel(VisualIdentitySettings settings)
    {
        var footerStatus = settings.FooterLogoStorageKey is not null
            ? VisualIdentityVariantStatus.Manual
            : settings.MainLogoSupportsRecoloring
                ? VisualIdentityVariantStatus.Automatic
                : VisualIdentityVariantStatus.Unavailable;
        var faviconStatus = settings.FaviconStorageKey is not null
            ? VisualIdentityVariantStatus.Manual
            : settings.MainLogoSupportsRecoloring
                ? VisualIdentityVariantStatus.Automatic
                : VisualIdentityVariantStatus.GenericFallback;
        var version = settings.AssetVersion;
        return new VisualIdentitySettingsModel(
            settings.PrimaryColor,
            settings.SecondaryColor,
            settings.AccentColor,
            settings.LogoColorsLinkedToPalette,
            settings.LogoColorStartOverride,
            settings.LogoColorEndOverride,
            settings.MainLogoSupportsRecoloring,
            AssetUrl(VisualIdentityAssetKind.MainLogo, version),
            footerStatus == VisualIdentityVariantStatus.Unavailable
                ? null
                : AssetUrl(VisualIdentityAssetKind.FooterLogo, version),
            footerStatus,
            settings.FooterLogoNeedsReview,
            AssetUrl(VisualIdentityAssetKind.Favicon, version),
            settings.FaviconStorageKey is not null ? GetContentType(settings.FaviconStorageKey) : "image/svg+xml",
            faviconStatus,
            settings.FaviconNeedsReview,
            version);
    }

    private static VisualIdentitySettings CreateDefaultSettings() => new()
    {
        Id = 1,
        PrimaryColor = VisualIdentityValidator.DefaultPrimaryColor,
        SecondaryColor = VisualIdentityValidator.DefaultSecondaryColor,
        AccentColor = VisualIdentityValidator.DefaultAccentColor,
        LogoColorsLinkedToPalette = true,
        MainLogoSupportsRecoloring = true,
        AssetVersion = Guid.Parse("00000000-0000-0000-0000-000000000001")
    };

    private static (string Start, string End) GetEffectiveLogoColors(VisualIdentitySettings settings) => settings.LogoColorsLinkedToPalette
        ? (settings.PrimaryColor, settings.SecondaryColor)
        : (settings.LogoColorStartOverride!, settings.LogoColorEndOverride!);

    private static string AssetUrl(VisualIdentityAssetKind kind, Guid version) =>
        $"/visual-identity/assets/{kind.ToString().ToLowerInvariant()}/{version:N}";

    private static string GetContentType(string key) => Path.GetExtension(key).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        _ => "image/svg+xml"
    };

    private static VisualIdentityAssetContent ToContent(XDocument document) => new(
        new MemoryStream(Encoding.UTF8.GetBytes(VisualIdentitySvgTransformer.Serialize(document))),
        "image/svg+xml");
}
