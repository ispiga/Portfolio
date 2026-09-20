namespace Portfolio.Application.Projects;

public sealed record ProjectReadModel(
    Guid Id,
    string Title,
    string Slug,
    string Summary,
    string? Description,
    string? RepositoryUrl,
    string? DemoUrl,
    string? PreviewImagePath,
    bool IsFeatured,
    int DisplayOrder);
