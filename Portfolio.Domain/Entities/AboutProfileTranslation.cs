namespace Portfolio.Domain.Entities;

public sealed class AboutProfileTranslation
{
    public int AboutProfileId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public AboutProfile AboutProfile { get; set; } = null!;
}