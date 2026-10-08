namespace Portfolio.Application.Certifications;

public sealed record CertificationAttachmentReadModel(
    Guid Id,
    Guid CertificationId,
    string OriginalFileName,
    string DisplayName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset CreatedAt,
    bool IsPublic);

public sealed record CertificationAttachmentContent(
    CertificationAttachmentReadModel Attachment,
    Stream Content);

public enum CertificationAttachmentError
{
    None,
    CertificationNotFound,
    UnsupportedType,
    TooLarge,
    InvalidContent,
    NotFound
}

public sealed record CertificationAttachmentOperationResult(
    CertificationAttachmentError Error,
    CertificationAttachmentReadModel? Attachment = null)
{
    public bool Succeeded => Error == CertificationAttachmentError.None;
}
