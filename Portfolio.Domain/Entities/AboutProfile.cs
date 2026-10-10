namespace Portfolio.Domain.Entities;

public sealed class AboutProfile
{
    public int Id { get; set; }

    public ICollection<AboutProfileTranslation> Translations { get; set; } = [];
}