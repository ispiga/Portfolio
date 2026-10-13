using System.ComponentModel.DataAnnotations;

namespace Portfolio.Infrastructure.Storage;

public sealed class VisualIdentityStorageOptions
{
    public const string SectionName = "Portfolio:VisualIdentityAssets";

    [Required]
    public string Directory { get; set; } = "App_Data/VisualIdentityAssets";

    [Range(1, 10485760)]
    public long MaximumFileSizeBytes { get; set; } = 10 * 1024 * 1024;
}
