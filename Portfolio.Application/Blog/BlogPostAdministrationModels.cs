using Portfolio.Domain.Entities;

namespace Portfolio.Application.Blog;

public sealed record BlogPostAdminListItem(
    Guid Id,
    string SpanishTitle,
    BlogPostEditorialStatus EditorialStatus,
    DateTimeOffset? PublishedOn,
    bool IsFeatured,
    bool HasSpanishTranslation,
    bool HasEnglishTranslation,
    int ImageCount);

public sealed record BlogPostAdminDetails(
    Guid Id,
    BlogPostEditorialStatus EditorialStatus,
    DateTimeOffset? PublishedOn,
    bool IsFeatured,
    string? FeaturedImagePath,
    BlogPostTranslationInput Spanish,
    BlogPostTranslationInput English,
    bool HasSpanishTranslation,
    bool HasEnglishTranslation);

public sealed record BlogPostTranslationInput(
    string Title,
    string Slug,
    string Excerpt,
    string Content,
    string? FeaturedImageAlt = null);

public sealed record BlogPostValidationError(string Field, string ResourceKey);

public sealed record BlogPostSaveResult(
    bool Succeeded,
    Guid? BlogPostId,
    IReadOnlyList<BlogPostValidationError> Errors);

public enum BlogPostDeleteResult
{
    Deleted,
    NotFound,
    HasImages
}
