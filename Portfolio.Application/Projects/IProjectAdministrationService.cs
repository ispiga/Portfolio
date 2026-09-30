namespace Portfolio.Application.Projects;

public interface IProjectAdministrationService
{
    Task<IReadOnlyList<ProjectAdminListItem>> GetProjectsAsync(CancellationToken cancellationToken = default);

    Task<ProjectAdminDetails?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProjectSaveResult> SaveAsync(ProjectEditRequest request, CancellationToken cancellationToken = default);

    Task<ProjectDeleteResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
