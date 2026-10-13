namespace Portfolio.Application.VisualIdentity;

public enum VisualIdentityAssetKind
{
    MainLogo,
    FooterLogo,
    Favicon
}

public enum VisualIdentityVariantStatus
{
    Automatic,
    Manual,
    GenericFallback,
    Unavailable
}

public sealed record VisualIdentityEditRequest(
    string PrimaryColor,
    string SecondaryColor,
    string AccentColor,
    bool LogoColorsLinkedToPalette,
    string? LogoColorStartOverride,
    string? LogoColorEndOverride);

public sealed record VisualIdentityValidationError(string Field, string ResourceKey);

public sealed record VisualIdentitySaveResult(bool Succeeded, IReadOnlyList<VisualIdentityValidationError> Errors);

public sealed record VisualIdentitySettingsModel(
    string PrimaryColor,
    string SecondaryColor,
    string AccentColor,
    bool LogoColorsLinkedToPalette,
    string? LogoColorStartOverride,
    string? LogoColorEndOverride,
    bool MainLogoSupportsRecoloring,
    string MainLogoUrl,
    string? FooterLogoUrl,
    VisualIdentityVariantStatus FooterLogoStatus,
    bool FooterLogoNeedsReview,
    string FaviconUrl,
    string FaviconContentType,
    VisualIdentityVariantStatus FaviconStatus,
    bool FaviconNeedsReview,
    Guid AssetVersion);

public enum VisualIdentityAssetError
{
    None,
    UnsupportedType,
    TooLarge,
    InvalidContent,
    NotFound
}

public sealed record VisualIdentityAssetUploadResult(
    VisualIdentityAssetError Error,
    string? StorageKey = null,
    string? ContentType = null,
    long SizeBytes = 0,
    bool SupportsAutomaticRecoloring = false)
{
    public bool Succeeded => Error == VisualIdentityAssetError.None;
}

public sealed record VisualIdentityAssetContent(Stream Content, string ContentType);
