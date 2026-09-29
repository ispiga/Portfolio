using System.ComponentModel.DataAnnotations;

namespace Portfolio.Infrastructure.Storage;

public sealed class ExperienceAttachmentStorageOptions
{
    public const string SectionName = "Portfolio:ExperienceAttachments";

    [Required]
    public string Directory { get; set; } = "App_Data/ExperienceAttachments";

    [Range(1, int.MaxValue)]
    public int MaximumFileCount { get; set; } = 5;

    [Range(1, long.MaxValue)]
    public long MaximumFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".pdf", ".jpg", ".jpeg", ".png"];
}
