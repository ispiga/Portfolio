namespace Portfolio.Application.Blog;

public interface IBlogPostQueryService
{
    Task<BlogPostReadModel?> GetFeaturedPostAsync(
        CancellationToken cancellationToken = default);
}
