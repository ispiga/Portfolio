namespace Portfolio.Application.Projects;

public sealed record ProjectAdminListItem(
    Guid Id,
    string SpanishTitle,
    int DisplayOrder,
    bool IsFeatured,
    bool HasSpanishTranslation,
    bool HasEnglishTranslation,
    bool HasPreviewImage);

public sealed record ProjectAdminDetails(
    Guid Id,
    string? RepositoryUrl,
    string? DemoUrl,
    string? PreviewImagePath,
    bool IsFeatured,
    int DisplayOrder,
    ProjectTranslationInput Spanish,
    ProjectTranslationInput English,
    bool HasSpanishTranslation,
    bool HasEnglishTranslation);

public sealed record ProjectValidationError(string Field, string ResourceKey);

public sealed record ProjectSaveResult(
    bool Succeeded,
    Guid? ProjectId,
    IReadOnlyList<ProjectValidationError> Errors);

public enum ProjectDeleteResult
{
    Deleted,
    NotFound
}
