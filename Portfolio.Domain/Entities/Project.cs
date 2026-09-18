namespace Portfolio.Domain.Entities;

public sealed class Project
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? RepositoryUrl { get; set; }

    public string? DemoUrl { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }
}
