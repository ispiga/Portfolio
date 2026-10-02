namespace Portfolio.Domain.Entities;

public sealed class BlogPost
{
    public Guid Id { get; set; }

    public string? FeaturedImagePath { get; set; }

    public DateTimeOffset? PublishedOn { get; set; }

    public BlogPostEditorialStatus EditorialStatus { get; set; }

    public bool IsFeatured { get; set; }

    public ICollection<BlogPostTranslation> Translations { get; set; } = [];

    public ICollection<BlogPostImage> Images { get; set; } = [];
}

public enum BlogPostEditorialStatus
{
    Draft = 0,
    ReadyToPublish = 1,
    Published = 2
}

public sealed class BlogPostImage
{
    public Guid Id { get; set; }

    public Guid BlogPostId { get; set; }

    public string StorageKey { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public BlogPost BlogPost { get; set; } = null!;
}
