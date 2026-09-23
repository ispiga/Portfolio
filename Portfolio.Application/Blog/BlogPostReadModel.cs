namespace Portfolio.Application.Blog;

public sealed record BlogPostReadModel(
    Guid Id,
    string? FeaturedImagePath,
    string FeaturedImageAlt,
    string Title,
    string Slug,
    string Excerpt,
    string Content,
    DateTimeOffset PublishedOn,
    bool IsFeatured);
