using Portfolio.Domain.Entities;

namespace Portfolio.Application.Blog;

public static class BlogPostTranslationSelector
{
    public static IReadOnlyList<BlogPostReadModel> SelectHomePosts(
        IEnumerable<BlogPost> blogPosts,
        string cultureName,
        int maximumCount)
    {
        if (maximumCount <= 0)
        {
            return [];
        }

        return blogPosts
            .OrderByDescending(blogPost => blogPost.IsFeatured)
            .ThenByDescending(blogPost => blogPost.PublishedOn)
            .ThenBy(blogPost => blogPost.Id)
            .Select(blogPost => Select(blogPost, cultureName))
            .OfType<BlogPostReadModel>()
            .Take(maximumCount)
            .ToArray();
    }

    public static IReadOnlyList<BlogPostReadModel> SelectPublishedPosts(
        IEnumerable<BlogPost> blogPosts,
        string cultureName)
    {
        return blogPosts
            .OrderByDescending(blogPost => blogPost.PublishedOn)
            .ThenBy(blogPost => blogPost.Id)
            .Select(blogPost => Select(blogPost, cultureName))
            .OfType<BlogPostReadModel>()
            .ToArray();
    }

    public static BlogPostReadModel? SelectBySlug(
        IEnumerable<BlogPost> blogPosts,
        string slug,
        string cultureName)
    {
        var culture = GetCultureCode(cultureName);
        var candidates = blogPosts.ToArray();
        var localizedMatch = candidates
            .Where(blogPost => blogPost.Translations.Any(translation =>
                string.Equals(translation.LanguageCode, culture, StringComparison.OrdinalIgnoreCase)
                && string.Equals(translation.Slug, slug, StringComparison.OrdinalIgnoreCase)))
            .Select(blogPost => Select(blogPost, cultureName))
            .OfType<BlogPostReadModel>()
            .FirstOrDefault();
        if (localizedMatch is not null || culture == "es-ES")
        {
            return localizedMatch;
        }

        return candidates
            .Where(blogPost => !blogPost.Translations.Any(translation =>
                string.Equals(translation.LanguageCode, culture, StringComparison.OrdinalIgnoreCase)))
            .Select(blogPost => Select(blogPost, cultureName))
            .OfType<BlogPostReadModel>()
            .FirstOrDefault(post => string.Equals(post.Slug, slug, StringComparison.OrdinalIgnoreCase));
    }

    public static BlogPostReadModel? SelectFeatured(
        IEnumerable<BlogPost> blogPosts,
        string cultureName)
    {
        return blogPosts
            .OrderByDescending(blogPost => blogPost.IsFeatured)
            .ThenByDescending(blogPost => blogPost.PublishedOn)
            .ThenBy(blogPost => blogPost.Id)
            .Select(blogPost => Select(blogPost, cultureName))
            .OfType<BlogPostReadModel>()
            .FirstOrDefault();
    }

    public static BlogPostReadModel? Select(BlogPost blogPost, string cultureName)
    {
        var culture = GetCultureCode(cultureName);

        var translation = blogPost.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, culture, StringComparison.OrdinalIgnoreCase))
            ?? blogPost.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, "es-ES", StringComparison.OrdinalIgnoreCase));

        if (translation is null
            || string.IsNullOrWhiteSpace(translation.Title)
            || string.IsNullOrWhiteSpace(translation.Slug)
            || string.IsNullOrWhiteSpace(translation.Excerpt)
            || string.IsNullOrWhiteSpace(translation.Content)
            || blogPost.EditorialStatus != BlogPostEditorialStatus.Published
            || blogPost.PublishedOn is not { } publishedOn)
        {
            return null;
        }

        if (publishedOn > DateTimeOffset.UtcNow)
        {
            return null;
        }

        return new BlogPostReadModel(
            blogPost.Id,
            blogPost.FeaturedImagePath,
            string.IsNullOrWhiteSpace(translation.FeaturedImageAlt)
                ? translation.Title
                : translation.FeaturedImageAlt,
            translation.Title,
            translation.Slug,
            translation.Excerpt,
            translation.Content,
            publishedOn,
            blogPost.IsFeatured);
    }

    private static string GetCultureCode(string cultureName) =>
        string.Equals(cultureName, "en-US", StringComparison.OrdinalIgnoreCase)
            ? "en-US"
            : "es-ES";
}
