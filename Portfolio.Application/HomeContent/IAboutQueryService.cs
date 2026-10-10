namespace Portfolio.Application.HomeContent;

public interface IAboutQueryService
{
    Task<AboutContentReadModel> GetAsync(CancellationToken cancellationToken = default);
}