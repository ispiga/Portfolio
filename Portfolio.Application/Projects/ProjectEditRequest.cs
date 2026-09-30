namespace Portfolio.Application.Projects;

public sealed record ProjectEditRequest(
    Guid? Id,
    string? RepositoryUrl,
    string? DemoUrl,
    bool IsFeatured,
    int DisplayOrder,
    ProjectTranslationInput Spanish,
    ProjectTranslationInput English);

public sealed record ProjectTranslationInput(
    string Title,
    string? Slug,
    string Summary,
    string? Description);
