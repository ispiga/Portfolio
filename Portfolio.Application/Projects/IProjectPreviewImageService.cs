namespace Portfolio.Application.Projects;

public interface IProjectPreviewImageService
{
    long MaximumFileSizeBytes { get; }

    Task<ProjectImageOperationResult> UploadAsync(
        Guid projectId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid projectId, CancellationToken cancellationToken = default);

    Task<ProjectImageContent?> OpenPublicReadAsync(Guid projectId, CancellationToken cancellationToken = default);
}

public sealed record ProjectImageOperationResult(ProjectImageError Error, string? ImagePath = null);

public sealed record ProjectImageContent(Stream Content, string ContentType);

public enum ProjectImageError
{
    None,
    ProjectNotFound,
    UnsupportedType,
    TooLarge,
    InvalidContent
}
