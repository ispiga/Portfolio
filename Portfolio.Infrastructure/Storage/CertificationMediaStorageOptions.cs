using System.ComponentModel.DataAnnotations;

namespace Portfolio.Infrastructure.Storage;

public sealed class CertificationMediaStorageOptions
{
    public const string SectionName = "Portfolio:CertificationMedia";

    [Required]
    public string Directory { get; set; } = "App_Data/CertificationMedia";

    [Range(1, long.MaxValue)]
    public long MaximumFileSizeBytes { get; set; } = 10 * 1024 * 1024;

    public string[] AllowedExtensions { get; set; } = [".pdf", ".jpg", ".jpeg", ".png"];
}
