namespace Portfolio.Domain.Entities;

public sealed class CertificationAttachment
{
    public Guid Id { get; set; }

    public Guid CertificationId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool IsPublic { get; set; }

    public Certification Certification { get; set; } = null!;
}
