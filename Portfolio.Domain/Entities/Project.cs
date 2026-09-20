namespace Portfolio.Domain.Entities;

public sealed class Project
{
    public Guid Id { get; set; }

    public string? RepositoryUrl { get; set; }

    public string? DemoUrl { get; set; }

    public string? PreviewImagePath { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<ProjectTranslation> Translations { get; set; } = [];
}
