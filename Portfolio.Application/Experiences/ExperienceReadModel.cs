namespace Portfolio.Application.Experiences;

public sealed record ExperienceReadModel(
    Guid Id,
    string RoleTitle,
    string CompanyName,
    string Summary,
    DateOnly StartDate,
    DateOnly? EndDate,
    int DisplayOrder);