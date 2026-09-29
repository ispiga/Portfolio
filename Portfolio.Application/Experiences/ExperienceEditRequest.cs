namespace Portfolio.Application.Experiences;

public sealed record ExperienceEditRequest(
    Guid? Id,
    DateOnly StartDate,
    DateOnly? EndDate,
    int DisplayOrder,
    ExperienceTranslationInput Spanish,
    ExperienceTranslationInput English);

public sealed record ExperienceTranslationInput(
    string RoleTitle,
    string CompanyName,
    string Summary);
