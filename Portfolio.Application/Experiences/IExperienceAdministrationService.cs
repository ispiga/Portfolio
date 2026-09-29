namespace Portfolio.Application.Experiences;

public interface IExperienceAdministrationService
{
    Task<IReadOnlyList<ExperienceAdminListItem>> GetExperiencesAsync(CancellationToken cancellationToken = default);

    Task<ExperienceAdminDetails?> GetExperienceAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ExperienceSaveResult> SaveAsync(ExperienceEditRequest request, CancellationToken cancellationToken = default);

    Task<ExperienceDeleteResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
