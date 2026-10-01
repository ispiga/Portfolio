namespace Portfolio.Application.Certifications;

public sealed record CertificationReadModel(
    Guid Id,
    string Name,
    string Issuer,
    string? Details,
    DateOnly? IssuedOn,
    string? CredentialUrl,
    string? CredentialId,
    int? Hours,
    string? ImagePath,
    int DisplayOrder,
    IReadOnlyList<CertificationAttachmentReadModel> Attachments);
