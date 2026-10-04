using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Blog;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Services;

public sealed class BlogPostQueryService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory) : IBlogPostQueryService
{
    public async Task<IReadOnlyList<BlogPostReadModel>> GetHomePostsAsync(
        int maximumCount = 3,
        CancellationToken cancellationToken = default)
    {
        if (maximumCount <= 0)
        {
            return [];
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var blogPosts = await GetPublicPosts(context)
            .Include(blogPost => blogPost.Translations)
            .ToListAsync(cancellationToken);

        return BlogPostTranslationSelector.SelectHomePosts(
            blogPosts,
            CultureInfo.CurrentUICulture.Name,
            maximumCount);
    }

    public async Task<IReadOnlyList<BlogPostReadModel>> GetPublishedPostsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var blogPosts = await GetPublicPosts(context)
            .Include(blogPost => blogPost.Translations)
            .ToListAsync(cancellationToken);

        return BlogPostTranslationSelector.SelectPublishedPosts(
            blogPosts,
            CultureInfo.CurrentUICulture.Name);
    }

    public async Task<BlogPostReadModel?> GetPublishedPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var cultureName = CultureInfo.CurrentUICulture.Name;
        var languageCode = string.Equals(cultureName, "en-US", StringComparison.OrdinalIgnoreCase)
            ? "en-US"
            : "es-ES";
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var query = GetPublicPosts(context);
        query = languageCode == "en-US"
            ? query.Where(blogPost => blogPost.Translations.Any(translation =>
                    translation.LanguageCode == languageCode && translation.Slug == slug)
                || (!blogPost.Translations.Any(translation => translation.LanguageCode == languageCode)
                    && blogPost.Translations.Any(translation =>
                        translation.LanguageCode == "es-ES" && translation.Slug == slug)))
            : query.Where(blogPost => blogPost.Translations.Any(translation =>
                translation.LanguageCode == languageCode && translation.Slug == slug));

        var blogPosts = await query
            .Include(blogPost => blogPost.Translations)
            .ToListAsync(cancellationToken);

        return BlogPostTranslationSelector.SelectBySlug(blogPosts, slug, cultureName);
    }

    public async Task<BlogPostReadModel?> GetFeaturedPostAsync(
        CancellationToken cancellationToken = default) =>
        (await GetHomePostsAsync(1, cancellationToken)).FirstOrDefault();

    private static IQueryable<BlogPost> GetPublicPosts(PortfolioDbContext context)
    {
        var now = DateTimeOffset.UtcNow;
        return context.BlogPosts
            .AsNoTracking()
            .Where(blogPost => blogPost.EditorialStatus == BlogPostEditorialStatus.Published
                && blogPost.PublishedOn.HasValue
                && blogPost.PublishedOn <= now);
    }
}
