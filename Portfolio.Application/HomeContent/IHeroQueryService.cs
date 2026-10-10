namespace Portfolio.Application.HomeContent;

public interface IHeroQueryService
{
    Task<HeroReadModel?> GetAsync(CancellationToken cancellationToken = default);
}