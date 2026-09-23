using Portfolio.Domain.Entities;

namespace Portfolio.Application.Blog;

public static class BlogPostTranslationSelector
{
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
        var culture = string.Equals(cultureName, "en-US", StringComparison.OrdinalIgnoreCase)
            ? "en-US"
            : "es-ES";

        var translation = blogPost.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, culture, StringComparison.OrdinalIgnoreCase))
            ?? blogPost.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, "es-ES", StringComparison.OrdinalIgnoreCase));

        if (translation is null
            || string.IsNullOrWhiteSpace(translation.Title)
            || string.IsNullOrWhiteSpace(translation.Slug)
            || string.IsNullOrWhiteSpace(translation.Excerpt)
            || string.IsNullOrWhiteSpace(translation.Content)
            || !blogPost.IsPublished
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
}
