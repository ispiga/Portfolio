namespace Portfolio.Application.Blog;

public interface IBlogPostImageService
{
    long MaximumFileSizeBytes { get; }

    Task<IReadOnlyList<BlogPostImageReadModel>> GetImagesAsync(
        Guid blogPostId,
        CancellationToken cancellationToken = default);

    Task<BlogPostImageOperationResult> UploadImageAsync(
        Guid blogPostId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<BlogPostImageOperationResult> SetFeaturedImageAsync(
        Guid imageId,
        CancellationToken cancellationToken = default);

    Task<BlogPostImageError> DeleteImageAsync(
        Guid imageId,
        CancellationToken cancellationToken = default);

    Task<BlogPostImageContent?> OpenImageAsync(
        Guid imageId,
        bool administratorCanViewDrafts,
        CancellationToken cancellationToken = default);
}
