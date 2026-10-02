namespace Portfolio.Application.Blog;

public interface IBlogPostAdministrationService
{
    Task<IReadOnlyList<BlogPostAdminListItem>> GetBlogPostsAsync(
        CancellationToken cancellationToken = default);

    Task<BlogPostAdminDetails?> GetBlogPostAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<BlogPostSaveResult> SaveAsync(
        BlogPostEditRequest request,
        CancellationToken cancellationToken = default);

    Task<BlogPostDeleteResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
