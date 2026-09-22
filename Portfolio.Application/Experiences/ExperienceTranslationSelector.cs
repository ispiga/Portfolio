using Portfolio.Domain.Entities;

namespace Portfolio.Application.Experiences;

public static class ExperienceTranslationSelector
{
    public static ExperienceReadModel? Select(Experience experience, string cultureName)
    {
        var culture = string.Equals(cultureName, "en-US", StringComparison.OrdinalIgnoreCase)
            ? "en-US"
            : "es-ES";

        var translation = experience.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, culture, StringComparison.OrdinalIgnoreCase))
            ?? experience.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, "es-ES", StringComparison.OrdinalIgnoreCase));

        if (translation is null
            || string.IsNullOrWhiteSpace(translation.RoleTitle)
            || string.IsNullOrWhiteSpace(translation.CompanyName)
            || string.IsNullOrWhiteSpace(translation.Summary))
        {
            return null;
        }

        return new ExperienceReadModel(
            experience.Id,
            translation.RoleTitle,
            translation.CompanyName,
            translation.Summary,
            experience.StartDate,
            experience.EndDate,
            experience.DisplayOrder);
    }
}