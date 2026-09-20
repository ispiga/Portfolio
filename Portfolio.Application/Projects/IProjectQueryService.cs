namespace Portfolio.Application.Projects;

public interface IProjectQueryService
{
    Task<IReadOnlyList<ProjectReadModel>> GetProjectsAsync(CancellationToken cancellationToken = default);
}
