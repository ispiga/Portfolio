namespace Portfolio.Application.Experiences;

public sealed record ExperienceAdminListItem(
    Guid Id,
    string SpanishRoleTitle,
    string SpanishCompanyName,
    DateOnly StartDate,
    DateOnly? EndDate,
    int DisplayOrder,
    bool HasSpanishTranslation,
    bool HasEnglishTranslation);

public sealed record ExperienceAdminDetails(
    Guid Id,
    DateOnly StartDate,
    DateOnly? EndDate,
    int DisplayOrder,
    ExperienceTranslationInput Spanish,
    ExperienceTranslationInput English,
    bool HasSpanishTranslation,
    bool HasEnglishTranslation);

public sealed record ExperienceValidationError(string Field, string ResourceKey);

public sealed record ExperienceSaveResult(
    bool Succeeded,
    Guid? ExperienceId,
    IReadOnlyList<ExperienceValidationError> Errors);

public enum ExperienceDeleteResult
{
    Deleted,
    NotFound,
    HasAttachments
}
