using System.ComponentModel.DataAnnotations;

namespace Portfolio.Infrastructure.Storage;

public sealed class BlogPostImageStorageOptions
{
    public const string SectionName = "Portfolio:BlogImages";

    [Required]
    public string Directory { get; set; } = "App_Data/BlogImages";

    [Range(1, long.MaxValue)]
    public long MaximumFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".svg"];
}
