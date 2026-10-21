namespace Portfolio.Application.Blog;

// Returned Content is sanitized before it can reach the public MarkupString renderer.
public interface IBlogPostQueryService
{
    Task<IReadOnlyList<BlogPostReadModel>> GetHomePostsAsync(
        int maximumCount = 3,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BlogPostReadModel>> GetPublishedPostsAsync(
        CancellationToken cancellationToken = default);

    Task<BlogPostReadModel?> GetPublishedPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<BlogPostReadModel?> GetFeaturedPostAsync(
        CancellationToken cancellationToken = default);
}
