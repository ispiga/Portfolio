namespace Portfolio.Application.Certifications;

public sealed record CertificationEditRequest(
    Guid? Id,
    DateOnly? IssuedOn,
    string? CredentialUrl,
    string? CredentialId,
    int? Hours,
    int DisplayOrder,
    CertificationTranslationInput Spanish,
    CertificationTranslationInput English);
