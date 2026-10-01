namespace Portfolio.Application.Certifications;

public static class CertificationEditValidator
{
    public static IReadOnlyList<CertificationValidationError> Validate(CertificationEditRequest request)
    {
        var errors = new List<CertificationValidationError>();
        if (request.DisplayOrder < 0)
        {
            errors.Add(new(nameof(request.DisplayOrder), "CertificationDisplayOrderInvalid"));
        }

        ValidateUrl(request.CredentialUrl, errors);
        if (request.CredentialId?.Length > 200)
        {
            errors.Add(new(nameof(request.CredentialId), "CertificationFieldTooLong"));
        }

        if (request.Hours < 0)
        {
            errors.Add(new(nameof(request.Hours), "CertificationHoursInvalid"));
        }

        ValidateTranslation(request.Spanish, nameof(request.Spanish), required: true, errors);
        ValidateTranslation(request.English, nameof(request.English), required: false, errors);
        return errors;
    }

    private static void ValidateUrl(string? value, ICollection<CertificationValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (value.Length > 500)
        {
            errors.Add(new(nameof(CertificationEditRequest.CredentialUrl), "CertificationFieldTooLong"));
        }
        else if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)
                 || uri.Scheme is not ("http" or "https"))
        {
            errors.Add(new(nameof(CertificationEditRequest.CredentialUrl), "CertificationUrlInvalid"));
        }
    }

    private static void ValidateTranslation(
        CertificationTranslationInput translation,
        string fieldPrefix,
        bool required,
        ICollection<CertificationValidationError> errors)
    {
        var fields = new (string Name, string Value, int MaxLength)[]
        {
            (nameof(translation.Name), translation.Name, 200),
            (nameof(translation.Issuer), translation.Issuer, 200)
        };
        var hasAnyValue = fields.Any(field => !string.IsNullOrWhiteSpace(field.Value))
            || !string.IsNullOrWhiteSpace(translation.Details);
        if (!required && !hasAnyValue)
        {
            return;
        }

        if (translation.Details?.Length > 1000)
        {
            errors.Add(new($"{fieldPrefix}.{nameof(translation.Details)}", "CertificationFieldTooLong"));
        }

        foreach (var field in fields)
        {
            if (string.IsNullOrWhiteSpace(field.Value))
            {
                errors.Add(new($"{fieldPrefix}.{field.Name}", "CertificationFieldRequired"));
            }
            else if (field.Value.Length > field.MaxLength)
            {
                errors.Add(new($"{fieldPrefix}.{field.Name}", "CertificationFieldTooLong"));
            }
        }
    }
}
