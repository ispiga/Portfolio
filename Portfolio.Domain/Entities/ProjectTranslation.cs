namespace Portfolio.Domain.Entities;

public sealed class ProjectTranslation
{
    public Guid ProjectId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Project Project { get; set; } = null!;
}
