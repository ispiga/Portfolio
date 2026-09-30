using System.ComponentModel.DataAnnotations;

namespace Portfolio.Infrastructure.Storage;

public sealed class ProjectPreviewImageStorageOptions
{
    public const string SectionName = "Portfolio:ProjectPreviewImages";

    [Required]
    public string Directory { get; set; } = "App_Data/ProjectPreviewImages";

    [Range(1, long.MaxValue)]
    public long MaximumFileSizeBytes { get; set; } = 10 * 1024 * 1024;
}
