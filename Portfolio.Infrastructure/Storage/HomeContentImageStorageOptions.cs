using System.ComponentModel.DataAnnotations;

namespace Portfolio.Infrastructure.Storage;

public sealed class HomeContentImageStorageOptions
{
    public const string SectionName = "Portfolio:HomeContentImages";

    [Required]
    public string Directory { get; set; } = "App_Data/HomeContentImages";

    [Range(1, long.MaxValue)]
    public long MaximumFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".svg", ".jpg", ".jpeg", ".png"];
}