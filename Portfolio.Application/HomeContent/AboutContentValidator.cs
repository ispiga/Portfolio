namespace Portfolio.Application.HomeContent;

public static class AboutContentValidator
{
    public static IReadOnlyList<AboutValidationError> Validate(AboutContentEditRequest request)
    {
        var errors = new List<AboutValidationError>();
        ValidateLocalized(request.SpanishProfile, request.EnglishProfile, 4000, "Profile", errors);
        if (string.IsNullOrWhiteSpace(request.SpanishProfile))
        {
            errors.Add(new("SpanishProfile", "AboutSpanishProfileRequired"));
        }

        var ids = new HashSet<Guid>();
        for (var groupIndex = 0; groupIndex < request.Groups.Count; groupIndex++)
        {
            var group = request.Groups[groupIndex];
            var groupField = $"Groups[{groupIndex}]";
            if (group.Id == Guid.Empty || !ids.Add(group.Id))
            {
                errors.Add(new($"{groupField}.Id", "AboutIdInvalid"));
            }

            ValidateLocalized(group.SpanishName, group.EnglishName, 100, $"{groupField}.Name", errors);
            if (string.IsNullOrWhiteSpace(group.SpanishName))
            {
                errors.Add(new($"{groupField}.SpanishName", "AboutSpanishNameRequired"));
            }

            if (group.DisplayOrder < 0)
            {
                errors.Add(new($"{groupField}.DisplayOrder", "HomeOrderInvalid"));
            }

            for (var skillIndex = 0; skillIndex < group.Skills.Count; skillIndex++)
            {
                var skill = group.Skills[skillIndex];
                var skillField = $"{groupField}.Skills[{skillIndex}]";
                if (skill.Id == Guid.Empty || !ids.Add(skill.Id))
                {
                    errors.Add(new($"{skillField}.Id", "AboutIdInvalid"));
                }

                ValidateLocalized(skill.SpanishName, skill.EnglishName, 100, $"{skillField}.Name", errors);
                if (string.IsNullOrWhiteSpace(skill.SpanishName))
                {
                    errors.Add(new($"{skillField}.SpanishName", "AboutSpanishNameRequired"));
                }

                if (skill.DisplayOrder < 0)
                {
                    errors.Add(new($"{skillField}.DisplayOrder", "HomeOrderInvalid"));
                }
            }
        }

        var hobbyIds = new HashSet<Guid>();
        for (var index = 0; index < request.Hobbies.Count; index++)
        {
            var hobby = request.Hobbies[index];
            var field = $"Hobbies[{index}]";
            if (hobby.Id == Guid.Empty || !hobbyIds.Add(hobby.Id) || ids.Contains(hobby.Id))
            {
                errors.Add(new($"{field}.Id", "AboutIdInvalid"));
            }

            if (string.IsNullOrWhiteSpace(hobby.StorageKey) || hobby.StorageKey.Length > 255)
            {
                errors.Add(new($"{field}.StorageKey", "HomeImageRequired"));
            }

            if (!HeroContentValidator.IsSupportedImage(hobby.ContentType)
                || hobby.SizeBytes is <= 0 or > HeroContentValidator.MaximumImageSizeBytes)
            {
                errors.Add(new($"{field}.Image", hobby.SizeBytes > HeroContentValidator.MaximumImageSizeBytes
                    ? "HomeImageTooLarge"
                    : "HomeImageInvalid"));
            }

            ValidateLocalized(hobby.SpanishName, hobby.EnglishName, 150, $"{field}.Name", errors);
            ValidateLocalized(hobby.SpanishDescription, hobby.EnglishDescription, 1200, $"{field}.Description", errors);
            if (string.IsNullOrWhiteSpace(hobby.SpanishName) || string.IsNullOrWhiteSpace(hobby.SpanishDescription))
            {
                errors.Add(new($"{field}.Spanish", "AboutSpanishHobbyRequired"));
            }

            if (string.IsNullOrWhiteSpace(hobby.EnglishName) != string.IsNullOrWhiteSpace(hobby.EnglishDescription))
            {
                errors.Add(new($"{field}.English", "AboutEnglishHobbyIncomplete"));
            }

            if (hobby.DisplayOrder < 0)
            {
                errors.Add(new($"{field}.DisplayOrder", "HomeOrderInvalid"));
            }
        }

        return errors;
    }

    private static void ValidateLocalized(
        string? spanish,
        string? english,
        int maximumLength,
        string field,
        ICollection<AboutValidationError> errors)
    {
        if (spanish?.Trim().Length > maximumLength || english?.Trim().Length > maximumLength)
        {
            errors.Add(new(field, "AboutTextTooLong"));
        }
    }
}