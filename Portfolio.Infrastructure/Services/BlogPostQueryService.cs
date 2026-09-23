using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Blog;

namespace Portfolio.Infrastructure.Services;

public sealed class BlogPostQueryService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory) : IBlogPostQueryService
{
    public async Task<BlogPostReadModel?> GetFeaturedPostAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var blogPosts = await context.BlogPosts
            .AsNoTracking()
            .Where(blogPost => blogPost.IsPublished
                && blogPost.PublishedOn.HasValue
                && blogPost.PublishedOn <= now)
            .Include(blogPost => blogPost.Translations)
            .OrderByDescending(blogPost => blogPost.IsFeatured)
            .ThenByDescending(blogPost => blogPost.PublishedOn)
            .ThenBy(blogPost => blogPost.Id)
            .ToListAsync(cancellationToken);

        return BlogPostTranslationSelector.SelectFeatured(
            blogPosts,
            CultureInfo.CurrentUICulture.Name);
    }
}
