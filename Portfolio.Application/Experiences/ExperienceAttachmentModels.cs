namespace Portfolio.Application.Experiences;

public sealed record ExperienceAttachmentReadModel(
    Guid Id,
    Guid ExperienceId,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTimeOffset CreatedAt,
    bool IsPublic);

public sealed record ExperienceAttachmentPreviewReadModel(
    Guid Id,
    string OriginalFileName,
    string ContentType);

public sealed record ExperienceAttachmentContent(
    ExperienceAttachmentReadModel Attachment,
    Stream Content);

public enum ExperienceAttachmentError
{
    None,
    ExperienceNotFound,
    TooManyFiles,
    UnsupportedType,
    TooLarge,
    InvalidContent,
    NotFound
}

public sealed record ExperienceAttachmentOperationResult(
    ExperienceAttachmentError Error,
    ExperienceAttachmentReadModel? Attachment = null)
{
    public bool Succeeded => Error == ExperienceAttachmentError.None;
}
