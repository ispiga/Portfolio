namespace Portfolio.Application.Certifications;

public interface ICertificationQueryService
{
    Task<IReadOnlyList<CertificationReadModel>> GetCertificationsAsync(
        CancellationToken cancellationToken = default);
}
