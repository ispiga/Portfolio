namespace Portfolio.Application.HomeContent;

public interface IAboutAdministrationService
{
    Task<AboutContentEditRequest> GetAsync(CancellationToken cancellationToken = default);

    Task<AboutSaveResult> SaveAsync(AboutContentEditRequest request, CancellationToken cancellationToken = default);
}