namespace Portfolio.Domain.Entities;

public sealed class BlogPost
{
    public Guid Id { get; set; }

    public string? FeaturedImagePath { get; set; }

    public DateTimeOffset? PublishedOn { get; set; }

    public bool IsPublished { get; set; }

    public bool IsFeatured { get; set; }

    public ICollection<BlogPostTranslation> Translations { get; set; } = [];
}
