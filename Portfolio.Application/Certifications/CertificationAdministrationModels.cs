namespace Portfolio.Application.Certifications;

public sealed record CertificationAdminListItem(
    Guid Id,
    string SpanishName,
    int DisplayOrder,
    bool HasSpanishTranslation,
    bool HasEnglishTranslation,
    bool HasImage,
    int AttachmentCount);

public sealed record CertificationAdminDetails(
    Guid Id,
    DateOnly? IssuedOn,
    string? CredentialUrl,
    string? CredentialId,
    int? Hours,
    string? ImagePath,
    int DisplayOrder,
    CertificationTranslationInput Spanish,
    CertificationTranslationInput English,
    bool HasSpanishTranslation,
    bool HasEnglishTranslation);

public sealed record CertificationTranslationInput(string Name, string Issuer, string? Details = null);

public sealed record CertificationValidationError(string Field, string ResourceKey);

public sealed record CertificationSaveResult(
    bool Succeeded,
    Guid? CertificationId,
    IReadOnlyList<CertificationValidationError> Errors);

public enum CertificationDeleteResult
{
    Deleted,
    NotFound
}
