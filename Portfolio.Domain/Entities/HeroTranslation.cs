namespace Portfolio.Domain.Entities;

public sealed class HeroTranslation
{
    public int HeroContentId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Headline { get; set; } = string.Empty;

    public string? Description { get; set; }

    public HeroContent HeroContent { get; set; } = null!;
}