using Microsoft.EntityFrameworkCore;
using Portfolio.Application.HomeContent;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Services;

public sealed class AboutQueryService(IDbContextFactory<PortfolioDbContext> dbContextFactory) : IAboutQueryService
{
    public async Task<AboutContentReadModel> GetAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var languageCode = HomeContentTranslationSelector.CurrentLanguageCode;
        var profile = await context.AboutProfiles.AsNoTracking()
            .Where(candidate => candidate.Id == 1)
            .SelectMany(candidate => candidate.Translations)
            .ToListAsync(cancellationToken);
        var groups = await context.SkillGroups.AsNoTracking()
            .Include(group => group.Translations)
            .Include(group => group.Skills).ThenInclude(skill => skill.Translations)
            .OrderBy(group => group.DisplayOrder).ThenBy(group => group.Id)
            .ToArrayAsync(cancellationToken);
        var hobbies = await context.Hobbies.AsNoTracking()
            .Include(hobby => hobby.Translations)
            .OrderBy(hobby => hobby.DisplayOrder).ThenBy(hobby => hobby.Id)
            .ToArrayAsync(cancellationToken);

        return new AboutContentReadModel(
            HomeContentTranslationSelector.Select(
                profile.SingleOrDefault(item => item.LanguageCode == languageCode)?.Description,
                profile.SingleOrDefault(item => item.LanguageCode == "es-ES")?.Description),
            groups.Select(group => new
                {
                    Model = group,
                    Name = SelectTranslation(group.Translations, languageCode, "es-ES")
                })
                .Where(item => item.Name is not null)
                .Select(item => new SkillGroupReadModel(
                    item.Name!,
                    item.Model.DisplayOrder,
                    item.Model.Skills
                        .Select(skill => new
                        {
                            Model = skill,
                            Name = SelectTranslation(skill.Translations, languageCode, "es-ES")
                        })
                        .Where(skill => skill.Name is not null)
                        .OrderBy(skill => skill.Model.DisplayOrder).ThenBy(skill => skill.Model.Id)
                        .Select(skill => new SkillReadModel(skill.Name!, skill.Model.DisplayOrder))
                        .ToArray()))
                .ToArray(),
            hobbies.Select(hobby => new
                {
                    Model = hobby,
                    Translation = SelectHobbyTranslation(hobby.Translations, languageCode)
                })
                .Where(item => item.Translation is not null)
                .Select(item => new HobbyReadModel(
                    item.Model.Id,
                    item.Translation!.Name,
                    item.Translation.Description,
                    $"/home-content-images/hobby/{item.Model.Id:N}",
                    item.Model.DisplayOrder))
                .ToArray());
    }

    private static string? SelectTranslation<TTranslation>(
        IEnumerable<TTranslation> translations,
        string languageCode,
        string fallbackLanguage) where TTranslation : class
    {
        var local = translations.FirstOrDefault(translation => GetLanguageCode(translation) == languageCode);
        var fallback = translations.FirstOrDefault(translation => GetLanguageCode(translation) == fallbackLanguage);
        return HomeContentTranslationSelector.Select(GetName(local), GetName(fallback));
    }

    private static HobbyTranslation? SelectHobbyTranslation(
        IEnumerable<HobbyTranslation> translations,
        string languageCode) => translations.FirstOrDefault(item => item.LanguageCode == languageCode)
            ?? translations.FirstOrDefault(item => item.LanguageCode == "es-ES");

    private static string? GetLanguageCode<TTranslation>(TTranslation translation) => translation switch
    {
        SkillGroupTranslation group => group.LanguageCode,
        SkillTranslation skill => skill.LanguageCode,
        _ => null
    };

    private static string? GetName<TTranslation>(TTranslation? translation) => translation switch
    {
        SkillGroupTranslation group => group.Name,
        SkillTranslation skill => skill.Name,
        _ => null
    };
}