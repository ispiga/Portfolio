namespace Portfolio.Domain.Entities;

public sealed class ExperienceTranslation
{
    public Guid ExperienceId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string RoleTitle { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public Experience Experience { get; set; } = null!;
}