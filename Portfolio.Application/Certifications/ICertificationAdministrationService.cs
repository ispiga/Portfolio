namespace Portfolio.Application.Certifications;

public interface ICertificationAdministrationService
{
    Task<IReadOnlyList<CertificationAdminListItem>> GetCertificationsAsync(
        CancellationToken cancellationToken = default);

    Task<CertificationAdminDetails?> GetCertificationAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CertificationSaveResult> SaveAsync(
        CertificationEditRequest request,
        CancellationToken cancellationToken = default);

    Task<CertificationDeleteResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
