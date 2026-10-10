namespace Portfolio.Domain.Entities;

public sealed class TechnologyLogo
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string AlternativeText { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public int DisplayOrder { get; set; }

    public int Orbit { get; set; } = 1;
}