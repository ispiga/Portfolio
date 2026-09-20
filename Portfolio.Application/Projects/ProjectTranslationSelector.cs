using Portfolio.Domain.Entities;

namespace Portfolio.Application.Projects;

public static class ProjectTranslationSelector
{
    public static ProjectReadModel? Select(Project project, string cultureName)
    {
        var culture = string.Equals(cultureName, "en-US", StringComparison.OrdinalIgnoreCase)
            ? "en-US"
            : "es-ES";

        var translation = project.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, culture, StringComparison.OrdinalIgnoreCase))
            ?? project.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, "es-ES", StringComparison.OrdinalIgnoreCase));

        if (translation is null
            || string.IsNullOrWhiteSpace(translation.Title)
            || string.IsNullOrWhiteSpace(translation.Slug)
            || string.IsNullOrWhiteSpace(translation.Summary))
        {
            return null;
        }

        return new ProjectReadModel(
            project.Id,
            translation.Title,
            translation.Slug,
            translation.Summary,
            translation.Description,
            project.RepositoryUrl,
            project.DemoUrl,
            project.PreviewImagePath,
            project.IsFeatured,
            project.DisplayOrder);
    }
}
