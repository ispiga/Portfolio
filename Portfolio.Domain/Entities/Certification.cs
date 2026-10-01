namespace Portfolio.Domain.Entities;

public sealed class Certification
{
    public Guid Id { get; set; }

    public DateOnly? IssuedOn { get; set; }

    public string? CredentialUrl { get; set; }

    public string? CredentialId { get; set; }

    public int? Hours { get; set; }

    public string? ImagePath { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<CertificationTranslation> Translations { get; set; } = [];

    public ICollection<CertificationAttachment> Attachments { get; set; } = [];
}
