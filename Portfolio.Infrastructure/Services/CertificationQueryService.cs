using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Certifications;

namespace Portfolio.Infrastructure.Services;

public sealed class CertificationQueryService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory) : ICertificationQueryService
{
    public async Task<IReadOnlyList<CertificationReadModel>> GetCertificationsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var certifications = await context.Certifications
            .AsNoTracking()
            .Include(certification => certification.Translations)
            .OrderBy(certification => certification.DisplayOrder)
            .ThenBy(certification => certification.Id)
            .ToListAsync(cancellationToken);

        return certifications
            .Select(certification => CertificationTranslationSelector.Select(
                certification,
                CultureInfo.CurrentUICulture.Name))
            .OfType<CertificationReadModel>()
            .ToArray();
    }
}
