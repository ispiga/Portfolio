using Microsoft.EntityFrameworkCore;
using Portfolio.Application.HomeContent;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Services;

public sealed class AboutAdministrationService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory,
    IHomeContentImageStorageService imageStorage) : IAboutAdministrationService
{
    public async Task<AboutContentEditRequest> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var profile = await context.AboutProfiles.AsNoTracking()
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == 1, cancellationToken);
        var groups = await context.SkillGroups.AsNoTracking()
            .Include(group => group.Translations)
            .Include(group => group.Skills).ThenInclude(skill => skill.Translations)
            .OrderBy(group => group.DisplayOrder).ThenBy(group => group.Id)
            .ToArrayAsync(cancellationToken);
        var hobbies = await context.Hobbies.AsNoTracking()
            .Include(hobby => hobby.Translations)
            .OrderBy(hobby => hobby.DisplayOrder).ThenBy(hobby => hobby.Id)
            .ToArrayAsync(cancellationToken);

        return new AboutContentEditRequest(
            profile?.Translations.SingleOrDefault(item => item.LanguageCode == "es-ES")?.Description ?? string.Empty,
            profile?.Translations.SingleOrDefault(item => item.LanguageCode == "en-US")?.Description,
            groups.Select(group => new SkillGroupEditModel(
                group.Id,
                FindTranslation(group.Translations, "es-ES")?.Name ?? string.Empty,
                FindTranslation(group.Translations, "en-US")?.Name,
                group.DisplayOrder,
                group.Skills.OrderBy(skill => skill.DisplayOrder).ThenBy(skill => skill.Id)
                    .Select(skill => new SkillEditModel(
                        skill.Id,
                        FindTranslation(skill.Translations, "es-ES")?.Name ?? string.Empty,
                        FindTranslation(skill.Translations, "en-US")?.Name,
                        skill.DisplayOrder)).ToArray())).ToArray(),
            hobbies.Select(hobby => new HobbyEditModel(
                hobby.Id,
                hobby.StorageKey,
                hobby.ContentType,
                hobby.SizeBytes,
                hobby.DisplayOrder,
                FindTranslation(hobby.Translations, "es-ES")?.Name ?? string.Empty,
                FindTranslation(hobby.Translations, "es-ES")?.Description ?? string.Empty,
                FindTranslation(hobby.Translations, "en-US")?.Name,
                FindTranslation(hobby.Translations, "en-US")?.Description)).ToArray());
    }

    public async Task<AboutSaveResult> SaveAsync(
        AboutContentEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = AboutContentValidator.Validate(request);
        if (errors.Count > 0)
        {
            return new(false, errors);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var profile = await context.AboutProfiles.Include(item => item.Translations)
            .SingleOrDefaultAsync(item => item.Id == 1, cancellationToken);
        if (profile is null)
        {
            profile = new AboutProfile { Id = 1 };
            context.AboutProfiles.Add(profile);
        }

        SetProfileTranslation(profile, "es-ES", request.SpanishProfile);
        SetProfileTranslation(profile, "en-US", request.EnglishProfile);

        var existingGroups = await context.SkillGroups
            .Include(group => group.Translations)
            .Include(group => group.Skills).ThenInclude(skill => skill.Translations)
            .ToDictionaryAsync(group => group.Id, cancellationToken);
        var requestedGroupIds = request.Groups.Select(group => group.Id).ToHashSet();
        foreach (var removed in existingGroups.Values.Where(group => !requestedGroupIds.Contains(group.Id)))
        {
            context.SkillGroups.Remove(removed);
        }

        var existingSkills = existingGroups.Values.SelectMany(group => group.Skills)
            .ToDictionary(skill => skill.Id);
        var requestedSkillIds = request.Groups.SelectMany(group => group.Skills).Select(skill => skill.Id).ToHashSet();
        foreach (var removed in existingSkills.Values.Where(skill => !requestedSkillIds.Contains(skill.Id)))
        {
            context.Skills.Remove(removed);
        }

        foreach (var inputGroup in request.Groups)
        {
            if (!existingGroups.TryGetValue(inputGroup.Id, out var group))
            {
                group = new SkillGroup { Id = inputGroup.Id };
                context.SkillGroups.Add(group);
            }

            group.DisplayOrder = inputGroup.DisplayOrder;
            SetGroupTranslation(group, "es-ES", inputGroup.SpanishName);
            SetGroupTranslation(group, "en-US", inputGroup.EnglishName);
            foreach (var inputSkill in inputGroup.Skills)
            {
                if (!existingSkills.TryGetValue(inputSkill.Id, out var skill))
                {
                    skill = new Skill { Id = inputSkill.Id, SkillGroupId = group.Id };
                    context.Skills.Add(skill);
                }
                else
                {
                    skill.SkillGroupId = group.Id;
                }

                skill.DisplayOrder = inputSkill.DisplayOrder;
                SetSkillTranslation(skill, "es-ES", inputSkill.SpanishName);
                SetSkillTranslation(skill, "en-US", inputSkill.EnglishName);
            }
        }

        var existingHobbies = await context.Hobbies.Include(hobby => hobby.Translations)
            .ToDictionaryAsync(hobby => hobby.Id, cancellationToken);
        var requestedHobbyIds = request.Hobbies.Select(hobby => hobby.Id).ToHashSet();
        var removedImages = existingHobbies.Values
            .Where(hobby => !requestedHobbyIds.Contains(hobby.Id)
                || !string.Equals(hobby.StorageKey, request.Hobbies.Single(item => item.Id == hobby.Id).StorageKey, StringComparison.Ordinal))
            .Select(hobby => (hobby.Id, hobby.StorageKey))
            .ToArray();
        foreach (var removed in existingHobbies.Values.Where(hobby => !requestedHobbyIds.Contains(hobby.Id)))
        {
            context.Hobbies.Remove(removed);
        }

        foreach (var input in request.Hobbies)
        {
            if (!existingHobbies.TryGetValue(input.Id, out var hobby))
            {
                hobby = new Hobby { Id = input.Id };
                context.Hobbies.Add(hobby);
            }

            hobby.StorageKey = input.StorageKey;
            hobby.ContentType = input.ContentType;
            hobby.SizeBytes = input.SizeBytes;
            hobby.DisplayOrder = input.DisplayOrder;
            SetHobbyTranslation(hobby, "es-ES", input.SpanishName, input.SpanishDescription);
            SetHobbyTranslation(hobby, "en-US", input.EnglishName, input.EnglishDescription);
        }

        await context.SaveChangesAsync(cancellationToken);
        foreach (var (id, key) in removedImages)
        {
            await imageStorage.DeleteAsync(HomeContentImageKind.Hobby, id, key);
        }

        return new(true, []);
    }

    private static TTranslation? FindTranslation<TTranslation>(
        IEnumerable<TTranslation> translations,
        string languageCode) where TTranslation : class => translations.SingleOrDefault(translation =>
            translation switch
            {
                SkillGroupTranslation groupTranslation => groupTranslation.LanguageCode == languageCode,
                SkillTranslation skillTranslation => skillTranslation.LanguageCode == languageCode,
                HobbyTranslation hobbyTranslation => hobbyTranslation.LanguageCode == languageCode,
                _ => false
            });

    private static void SetProfileTranslation(AboutProfile profile, string languageCode, string? value)
    {
        var translation = profile.Translations.SingleOrDefault(item => item.LanguageCode == languageCode);
        SetTranslationValue(value, translation, () => profile.Translations.Remove(translation!), () =>
            profile.Translations.Add(new AboutProfileTranslation
            {
                AboutProfileId = profile.Id,
                LanguageCode = languageCode,
                Description = value!.Trim()
            }), next => translation!.Description = next);
    }

    private static void SetGroupTranslation(SkillGroup group, string languageCode, string? value)
    {
        var translation = group.Translations.SingleOrDefault(item => item.LanguageCode == languageCode);
        SetTranslationValue(value, translation, () => group.Translations.Remove(translation!), () =>
            group.Translations.Add(new SkillGroupTranslation
            {
                SkillGroupId = group.Id,
                LanguageCode = languageCode,
                Name = value!.Trim()
            }), next => translation!.Name = next);
    }

    private static void SetSkillTranslation(Skill skill, string languageCode, string? value)
    {
        var translation = skill.Translations.SingleOrDefault(item => item.LanguageCode == languageCode);
        SetTranslationValue(value, translation, () => skill.Translations.Remove(translation!), () =>
            skill.Translations.Add(new SkillTranslation
            {
                SkillId = skill.Id,
                LanguageCode = languageCode,
                Name = value!.Trim()
            }), next => translation!.Name = next);
    }

    private static void SetHobbyTranslation(Hobby hobby, string languageCode, string? name, string? description)
    {
        var translation = hobby.Translations.SingleOrDefault(item => item.LanguageCode == languageCode);
        if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(description))
        {
            if (translation is not null)
            {
                hobby.Translations.Remove(translation);
            }
        }
        else if (translation is null)
        {
            hobby.Translations.Add(new HobbyTranslation
            {
                HobbyId = hobby.Id,
                LanguageCode = languageCode,
                Name = name!.Trim(),
                Description = description!.Trim()
            });
        }
        else
        {
            translation.Name = name!.Trim();
            translation.Description = description!.Trim();
        }
    }

    private static void SetTranslationValue(
        string? value,
        object? translation,
        Action remove,
        Action add,
        Action<string> update)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (translation is not null)
            {
                remove();
            }
        }
        else if (translation is null)
        {
            add();
        }
        else
        {
            update(value.Trim());
        }
    }
}