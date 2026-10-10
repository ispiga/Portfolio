namespace Portfolio.Domain.Entities;

public sealed class Hobby
{
    public Guid Id { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<HobbyTranslation> Translations { get; set; } = [];
}