namespace Portfolio.Application.Projects;

public static class ProjectEditValidator
{
    public static IReadOnlyList<ProjectValidationError> Validate(ProjectEditRequest request)
    {
        var errors = new List<ProjectValidationError>();
        if (request.DisplayOrder < 0)
        {
            errors.Add(new(nameof(request.DisplayOrder), "ProjectDisplayOrderInvalid"));
        }

        ValidateUrl(request.RepositoryUrl, nameof(request.RepositoryUrl), errors);
        ValidateUrl(request.DemoUrl, nameof(request.DemoUrl), errors);
        ValidateTranslation(request.Spanish, nameof(request.Spanish), required: true, errors);
        ValidateTranslation(request.English, nameof(request.English), required: false, errors);
        return errors;
    }

    private static void ValidateUrl(
        string? value,
        string field,
        ICollection<ProjectValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (value.Length > 500)
        {
            errors.Add(new(field, "ProjectFieldTooLong"));
        }
        else if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
                 || uri.Scheme is not ("http" or "https"))
        {
            errors.Add(new(field, "ProjectUrlInvalid"));
        }
    }

    private static void ValidateTranslation(
        ProjectTranslationInput translation,
        string fieldPrefix,
        bool required,
        ICollection<ProjectValidationError> errors)
    {
        var fields = new (string Name, string? Value, int MaxLength)[]
        {
            (nameof(translation.Title), translation.Title, 200),
            (nameof(translation.Summary), translation.Summary, 500)
        };
        var hasAnyValue = fields.Any(field => !string.IsNullOrWhiteSpace(field.Value))
            || !string.IsNullOrWhiteSpace(translation.Description);

        if (!required && !hasAnyValue)
        {
            return;
        }

        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field.Value))
            {
                errors.Add(new($"{fieldPrefix}.{field.Name}", "ProjectFieldRequired"));
            }
            else if (field.Value.Length > field.MaxLength)
            {
                errors.Add(new($"{fieldPrefix}.{field.Name}", "ProjectFieldTooLong"));
            }
        }
    }
}
