namespace Portfolio.Application.Experiences;

public static class ExperienceEditValidator
{
    public const int MaximumSummaryLength = 2000;

    public static IReadOnlyList<ExperienceValidationError> Validate(ExperienceEditRequest request)
    {
        var errors = new List<ExperienceValidationError>();
        if (request.StartDate == default)
        {
            errors.Add(new(nameof(request.StartDate), "ExperienceStartDateRequired"));
        }

        if (request.EndDate is { } endDate && endDate < request.StartDate)
        {
            errors.Add(new(nameof(request.EndDate), "ExperienceEndDateBeforeStart"));
        }

        if (request.DisplayOrder < 0)
        {
            errors.Add(new(nameof(request.DisplayOrder), "ExperienceDisplayOrderInvalid"));
        }

        ValidateTranslation(request.Spanish, nameof(request.Spanish), required: true, errors);
        ValidateTranslation(request.English, nameof(request.English), required: false, errors);
        return errors;
    }

    private static void ValidateTranslation(
        ExperienceTranslationInput translation,
        string fieldPrefix,
        bool required,
        ICollection<ExperienceValidationError> errors)
    {
        var fields = new (string Name, string Value, int MaxLength)[]
        {
            (nameof(translation.RoleTitle), translation.RoleTitle, 200),
            (nameof(translation.CompanyName), translation.CompanyName, 200),
            (nameof(translation.Summary), translation.Summary, MaximumSummaryLength)
        };
        var hasAnyValue = fields.Any(field => !string.IsNullOrWhiteSpace(field.Value));

        if (!required && !hasAnyValue)
        {
            return;
        }

        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field.Value))
            {
                errors.Add(new($"{fieldPrefix}.{field.Name}", "ExperienceFieldRequired"));
            }
            else if (field.Value.Length > field.MaxLength)
            {
                errors.Add(new($"{fieldPrefix}.{field.Name}", "ExperienceFieldTooLong"));
            }
        }
    }
}
