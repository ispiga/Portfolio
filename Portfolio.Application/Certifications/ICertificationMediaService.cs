namespace Portfolio.Application.Certifications;

public interface ICertificationMediaService
{
    long MaximumFileSizeBytes { get; }

    Task<IReadOnlyList<CertificationAttachmentReadModel>> GetAttachmentsAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default);

    Task<CertificationAttachmentOperationResult> UploadAttachmentAsync(
        Guid certificationId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAttachmentDisplayNameAsync(
        Guid attachmentId,
        string displayName,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAttachmentAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default);

    Task<CertificationAttachmentContent?> OpenAttachmentAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default);

    Task<CertificationCardImageOperationResult> UploadCardImageAsync(
        Guid certificationId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteCardImageAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default);

    Task<CertificationImageContent?> OpenCardImageAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default);

    Task DeleteCertificationFilesAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default);
}
