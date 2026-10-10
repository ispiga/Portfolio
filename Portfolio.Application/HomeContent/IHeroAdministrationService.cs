namespace Portfolio.Application.HomeContent;

public interface IHeroAdministrationService
{
    Task<HeroEditRequest> GetAsync(CancellationToken cancellationToken = default);

    Task<HeroSaveResult> SaveAsync(HeroEditRequest request, CancellationToken cancellationToken = default);
}