namespace Portfolio.Domain.Entities;

public sealed class ExperienceAttachment
{
    public Guid Id { get; set; }

    public Guid ExperienceId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool IsPublic { get; set; }

    public Experience Experience { get; set; } = null!;
}