using Portfolio.Domain.Entities;

namespace Portfolio.Application.Blog;

public sealed record BlogPostEditRequest(
    Guid? Id,
    BlogPostEditorialStatus EditorialStatus,
    bool IsFeatured,
    BlogPostTranslationInput Spanish,
    BlogPostTranslationInput English);
