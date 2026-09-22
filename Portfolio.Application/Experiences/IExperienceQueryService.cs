namespace Portfolio.Application.Experiences;

public interface IExperienceQueryService
{
    Task<IReadOnlyList<ExperienceReadModel>> GetExperiencesAsync(
        CancellationToken cancellationToken = default);
}