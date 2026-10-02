using Portfolio.Domain.Entities;

namespace Portfolio.Application.Blog;

public static class BlogPostEditValidator
{
    public static IReadOnlyList<BlogPostValidationError> Validate(BlogPostEditRequest request)
    {
        var errors = new List<BlogPostValidationError>();
        if (!Enum.IsDefined(request.EditorialStatus))
        {
            errors.Add(new(nameof(request.EditorialStatus), "BlogPostStatusInvalid"));
            return errors;
        }

        ValidateTranslation(request.Spanish, nameof(request.Spanish),
            request.EditorialStatus is BlogPostEditorialStatus.ReadyToPublish or BlogPostEditorialStatus.Published, errors);
        ValidateTranslation(request.English, nameof(request.English), required: false, errors);
        return errors;
    }

    private static void ValidateTranslation(
        BlogPostTranslationInput translation,
        string fieldPrefix,
        bool required,
        ICollection<BlogPostValidationError> errors)
    {
        ValidateLength(translation.Title, 200, fieldPrefix, nameof(translation.Title), errors);
        ValidateLength(translation.Slug, 200, fieldPrefix, nameof(translation.Slug), errors);
        ValidateLength(translation.Excerpt, 500, fieldPrefix, nameof(translation.Excerpt), errors);
        ValidateLength(translation.FeaturedImageAlt, 500, fieldPrefix, nameof(translation.FeaturedImageAlt), errors);

        var requiredFields = new (string Name, string Value)[]
        {
            (nameof(translation.Title), translation.Title),
            (nameof(translation.Slug), translation.Slug),
            (nameof(translation.Excerpt), translation.Excerpt),
            (nameof(translation.Content), translation.Content)
        };
        var hasAnyValue = requiredFields.Any(field => !string.IsNullOrWhiteSpace(field.Value))
            || !string.IsNullOrWhiteSpace(translation.FeaturedImageAlt);
        if (!required && !hasAnyValue)
        {
            return;
        }

        if (required)
        {
            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    errors.Add(new($"{fieldPrefix}.{field.Name}", "BlogPostFieldRequired"));
                }
            }
        }
    }

    private static void ValidateLength(
        string? value,
        int maximumLength,
        string fieldPrefix,
        string fieldName,
        ICollection<BlogPostValidationError> errors)
    {
        if (value?.Length > maximumLength)
        {
            errors.Add(new($"{fieldPrefix}.{fieldName}", "BlogPostFieldTooLong"));
        }
    }
}
