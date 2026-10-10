namespace Portfolio.Application.HomeContent;

public static class HeroContentValidator
{
    public const long MaximumImageSizeBytes = 10 * 1024 * 1024;
    public const int MaximumOrbitCount = 3;

    public static IReadOnlyList<HeroValidationError> Validate(HeroEditRequest request)
    {
        var errors = new List<HeroValidationError>();
        if (request.OrbitCount is < 1 or > MaximumOrbitCount)
        {
            errors.Add(new(nameof(request.OrbitCount), "HomeOrbitCountInvalid"));
        }

        ValidateHeadline(request.SpanishHeadline, "SpanishHeadline", required: true, errors);
        ValidateHeadline(request.EnglishHeadline, "EnglishHeadline", required: false, errors);

        if (request.ProfileImageId is null
            && (request.ProfileImageStorageKey is not null
                || request.ProfileImageContentType is not null
                || request.ProfileImageSizeBytes is not null
                || request.ProfileImageAlternativeText is not null)
            || request.ProfileImageId is not null
                && (request.ProfileImageId == Guid.Empty
                    || string.IsNullOrWhiteSpace(request.ProfileImageStorageKey)
                    || request.ProfileImageStorageKey.Length > 255
                    || !IsSupportedImage(request.ProfileImageContentType)
                    || request.ProfileImageSizeBytes is <= 0 or > MaximumImageSizeBytes
                    || string.IsNullOrWhiteSpace(request.ProfileImageAlternativeText)
                    || request.ProfileImageAlternativeText.Trim().Length > 200))
        {
            errors.Add(new("ProfileImage", request.ProfileImageSizeBytes > MaximumImageSizeBytes
                ? "HomeImageTooLarge"
                : "HomeImageInvalid"));
        }

        var ids = new HashSet<Guid>();
        for (var index = 0; index < request.Logos.Count; index++)
        {
            var logo = request.Logos[index];
            var prefix = $"Logos[{index}]";
            if (logo.Id == Guid.Empty || !ids.Add(logo.Id))
            {
                errors.Add(new($"{prefix}.Id", "HomeLogoIdInvalid"));
            }

            if (string.IsNullOrWhiteSpace(logo.Name) || logo.Name.Trim().Length > 100)
            {
                errors.Add(new($"{prefix}.Name", "HomeLogoNameInvalid"));
            }

            if (string.IsNullOrWhiteSpace(logo.AlternativeText) || logo.AlternativeText.Trim().Length > 200)
            {
                errors.Add(new($"{prefix}.AlternativeText", "HomeLogoAltInvalid"));
            }

            if (string.IsNullOrWhiteSpace(logo.StorageKey) || logo.StorageKey.Length > 255)
            {
                errors.Add(new($"{prefix}.StorageKey", "HomeImageRequired"));
            }

            if (!IsSupportedImage(logo.ContentType)
                || logo.SizeBytes is <= 0 or > MaximumImageSizeBytes)
            {
                errors.Add(new($"{prefix}.Image", logo.SizeBytes > MaximumImageSizeBytes
                    ? "HomeImageTooLarge"
                    : "HomeImageInvalid"));
            }

            if (logo.DisplayOrder < 0)
            {
                errors.Add(new($"{prefix}.DisplayOrder", "HomeOrderInvalid"));
            }

            if (logo.Orbit < 1 || logo.Orbit > request.OrbitCount)
            {
                errors.Add(new($"{prefix}.Orbit", "HomeLogoOrbitInvalid"));
            }
        }

        return errors;
    }

    public static bool IsSupportedImage(string? contentType) => contentType is
        "image/svg+xml" or "image/jpeg" or "image/png";

    private static void ValidateHeadline(
        string? value,
        string field,
        bool required,
        ICollection<HeroValidationError> errors)
    {
        if (required && string.IsNullOrWhiteSpace(value)
            || value?.Trim().Length > 250)
        {
            errors.Add(new(field, field == "SpanishHeadline" ? "HomeSpanishHeadlineRequired" : "HomeHeadlineTooLong"));
        }
    }
}