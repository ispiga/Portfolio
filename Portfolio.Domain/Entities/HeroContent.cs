namespace Portfolio.Domain.Entities;

public sealed class HeroContent
{
    public int Id { get; set; }

    public int OrbitCount { get; set; } = 1;

    public Guid? ProfileImageId { get; set; }

    public string? ProfileImageStorageKey { get; set; }

    public string? ProfileImageContentType { get; set; }

    public long? ProfileImageSizeBytes { get; set; }

    public string? ProfileImageAlternativeText { get; set; }

    public ICollection<HeroTranslation> Translations { get; set; } = [];
}