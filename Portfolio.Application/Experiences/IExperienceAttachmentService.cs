namespace Portfolio.Application.Experiences;

public interface IExperienceAttachmentService
{
    int MaximumFileCount { get; }

    long MaximumFileSizeBytes { get; }

    Task<IReadOnlyList<ExperienceAttachmentReadModel>> GetAttachmentsAsync(
        Guid experienceId,
        CancellationToken cancellationToken = default);

    Task<ExperienceAttachmentOperationResult> UploadAsync(
        Guid experienceId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken = default);

    Task<bool> SetPublicAsync(Guid attachmentId, bool isPublic, CancellationToken cancellationToken = default);

    Task<ExperienceAttachmentContent?> OpenReadAsync(Guid attachmentId, CancellationToken cancellationToken = default);

    Task<ExperienceAttachmentContent?> OpenPublicReadAsync(Guid attachmentId, CancellationToken cancellationToken = default);
}
