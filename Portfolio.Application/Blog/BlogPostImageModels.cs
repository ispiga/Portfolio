namespace Portfolio.Application.Blog;

public sealed record BlogPostImageReadModel(
    Guid Id,
    Guid BlogPostId,
    string ContentType,
    long SizeBytes,
    DateTimeOffset CreatedAt,
    bool IsFeatured);

public sealed record BlogPostImageContent(Stream Content, string ContentType);

public enum BlogPostImageError
{
    None,
    BlogPostNotFound,
    UnsupportedType,
    TooLarge,
    InvalidContent,
    NotFound,
    InUse
}

public sealed record BlogPostImageOperationResult(
    BlogPostImageError Error,
    BlogPostImageReadModel? Image = null)
{
    public bool Succeeded => Error == BlogPostImageError.None;
}
