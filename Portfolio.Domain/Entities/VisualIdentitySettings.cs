namespace Portfolio.Domain.Entities;

public sealed class VisualIdentitySettings
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;

    public string PrimaryColor { get; set; } = "#00A3FF";

    public string SecondaryColor { get; set; } = "#3E00D1";

    public string AccentColor { get; set; } = "#C98F42";

    public bool LogoColorsLinkedToPalette { get; set; } = true;

    public string? LogoColorStartOverride { get; set; }

    public string? LogoColorEndOverride { get; set; }

    public string? MainLogoStorageKey { get; set; }

    public long? MainLogoSizeBytes { get; set; }

    public bool MainLogoSupportsRecoloring { get; set; } = true;

    public string? FooterLogoStorageKey { get; set; }

    public long? FooterLogoSizeBytes { get; set; }

    public string? FaviconStorageKey { get; set; }

    public long? FaviconSizeBytes { get; set; }

    public bool FooterLogoNeedsReview { get; set; }

    public bool FaviconNeedsReview { get; set; }

    public Guid AssetVersion { get; set; } = Guid.NewGuid();
}
