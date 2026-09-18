namespace Portfolio.Domain.Entities;

public sealed class BlogPost
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Excerpt { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset? PublishedOn { get; set; }

    public bool IsPublished { get; set; }

    public bool IsFeatured { get; set; }
}
