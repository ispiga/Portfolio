using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Experiences;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Services;

public sealed class ExperienceAdministrationService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory) : IExperienceAdministrationService
{
    public async Task<IReadOnlyList<ExperienceAdminListItem>> GetExperiencesAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var experiences = await context.Experiences
            .AsNoTracking()
            .AsSplitQuery()
            .Include(experience => experience.Translations)
            .Include(experience => experience.Attachments)
            .OrderBy(experience => experience.DisplayOrder)
            .ThenBy(experience => experience.Id)
            .ToListAsync(cancellationToken);

        return experiences.Select(experience =>
        {
            var spanish = FindTranslation(experience, "es-ES");
            return new ExperienceAdminListItem(
                experience.Id,
                spanish?.RoleTitle ?? string.Empty,
                spanish?.CompanyName ?? string.Empty,
                experience.StartDate,
                experience.EndDate,
                experience.DisplayOrder,
                IsComplete(spanish),
                IsComplete(FindTranslation(experience, "en-US")),
                experience.Attachments.Count);
        }).ToArray();
    }

    public async Task<ExperienceAdminDetails?> GetExperienceAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var experience = await context.Experiences
            .AsNoTracking()
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        return experience is null ? null : ToDetails(experience);
    }

    public async Task<ExperienceSaveResult> SaveAsync(
        ExperienceEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = ExperienceEditValidator.Validate(request);
        if (errors.Count > 0)
        {
            return new(false, null, errors);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        Experience experience;
        if (request.Id is { } id)
        {
            experience = await context.Experiences
                .Include(candidate => candidate.Translations)
                .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken)!;
            if (experience is null)
            {
                return new(false, null, [new(nameof(request.Id), "ExperienceNotFound")]);
            }
        }
        else
        {
            experience = new Experience { Id = Guid.NewGuid() };
            context.Experiences.Add(experience);
        }

        experience.StartDate = request.StartDate;
        experience.EndDate = request.EndDate;
        experience.DisplayOrder = request.DisplayOrder;
        UpdateTranslation(experience, "es-ES", request.Spanish);
        UpdateTranslation(experience, "en-US", request.English);

        await context.SaveChangesAsync(cancellationToken);
        return new(true, experience.Id, []);
    }

    public async Task<ExperienceDeleteResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var experience = await context.Experiences.SingleOrDefaultAsync(
            candidate => candidate.Id == id,
            cancellationToken);
        if (experience is null)
        {
            return ExperienceDeleteResult.NotFound;
        }

        if (await context.ExperienceAttachments.AnyAsync(
                attachment => attachment.ExperienceId == id,
                cancellationToken))
        {
            return ExperienceDeleteResult.HasAttachments;
        }

        context.Experiences.Remove(experience);
        await context.SaveChangesAsync(cancellationToken);
        return ExperienceDeleteResult.Deleted;
    }

    private static ExperienceAdminDetails ToDetails(Experience experience)
    {
        var spanish = FindTranslation(experience, "es-ES");
        var english = FindTranslation(experience, "en-US");
        return new ExperienceAdminDetails(
            experience.Id,
            experience.StartDate,
            experience.EndDate,
            experience.DisplayOrder,
            ToInput(spanish),
            ToInput(english),
            IsComplete(spanish),
            IsComplete(english));
    }

    private static ExperienceTranslation? FindTranslation(Experience experience, string languageCode) =>
        experience.Translations.SingleOrDefault(translation => translation.LanguageCode == languageCode);

    private static bool IsComplete(ExperienceTranslation? translation) =>
        translation is not null
        && !string.IsNullOrWhiteSpace(translation.RoleTitle)
        && !string.IsNullOrWhiteSpace(translation.CompanyName)
        && !string.IsNullOrWhiteSpace(translation.Summary);

    private static ExperienceTranslationInput ToInput(ExperienceTranslation? translation) => new(
        translation?.RoleTitle ?? string.Empty,
        translation?.CompanyName ?? string.Empty,
        translation?.Summary ?? string.Empty);

    private static void UpdateTranslation(
        Experience experience,
        string languageCode,
        ExperienceTranslationInput input)
    {
        var translation = FindTranslation(experience, languageCode);
        if (string.IsNullOrWhiteSpace(input.RoleTitle)
            && string.IsNullOrWhiteSpace(input.CompanyName)
            && string.IsNullOrWhiteSpace(input.Summary))
        {
            if (translation is not null)
            {
                experience.Translations.Remove(translation);
            }

            return;
        }

        if (translation is null)
        {
            translation = new ExperienceTranslation { LanguageCode = languageCode };
            experience.Translations.Add(translation);
        }

        translation.RoleTitle = input.RoleTitle.Trim();
        translation.CompanyName = input.CompanyName.Trim();
        translation.Summary = input.Summary.Trim();
    }
}
