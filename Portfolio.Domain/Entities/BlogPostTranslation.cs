namespace Portfolio.Domain.Entities;

public sealed class BlogPostTranslation
{
    public Guid BlogPostId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Excerpt { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? FeaturedImageAlt { get; set; }

    public BlogPost BlogPost { get; set; } = null!;
}
