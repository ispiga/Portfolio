using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Certifications;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Services;

public sealed class CertificationAdministrationService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory,
    ICertificationMediaService mediaService) : ICertificationAdministrationService
{
    public async Task<IReadOnlyList<CertificationAdminListItem>> GetCertificationsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var certifications = await context.Certifications
            .AsNoTracking()
            .AsSplitQuery()
            .Include(certification => certification.Translations)
            .Include(certification => certification.Attachments)
            .OrderBy(certification => certification.DisplayOrder)
            .ThenBy(certification => certification.Id)
            .ToListAsync(cancellationToken);

        return certifications.Select(certification =>
        {
            var spanish = FindTranslation(certification, "es-ES");
            return new CertificationAdminListItem(
                certification.Id,
                spanish?.Name ?? string.Empty,
                certification.DisplayOrder,
                IsComplete(spanish),
                IsComplete(FindTranslation(certification, "en-US")),
                !string.IsNullOrWhiteSpace(certification.ImagePath),
                certification.Attachments.Count);
        }).ToArray();
    }

    public async Task<CertificationAdminDetails?> GetCertificationAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var certification = await context.Certifications
            .AsNoTracking()
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        return certification is null ? null : ToDetails(certification);
    }

    public async Task<CertificationSaveResult> SaveAsync(
        CertificationEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = CertificationEditValidator.Validate(request);
        if (errors.Count > 0)
        {
            return new(false, null, errors);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        Certification certification;
        if (request.Id is { } id)
        {
            var existing = await context.Certifications
                .Include(candidate => candidate.Translations)
                .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            if (existing is null)
            {
                return new(false, null, [new(nameof(request.Id), "CertificationNotFound")]);
            }

            certification = existing;
        }
        else
        {
            certification = new Certification { Id = Guid.NewGuid() };
            context.Certifications.Add(certification);
        }

        certification.IssuedOn = request.IssuedOn;
        certification.CredentialUrl = NormalizeOptional(request.CredentialUrl);
        certification.CredentialId = NormalizeOptional(request.CredentialId);
        certification.Hours = request.Hours;
        certification.DisplayOrder = request.DisplayOrder;
        UpdateTranslation(certification, "es-ES", request.Spanish);
        UpdateTranslation(certification, "en-US", request.English);

        await context.SaveChangesAsync(cancellationToken);
        return new(true, certification.Id, []);
    }

    public async Task<CertificationDeleteResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var certification = await context.Certifications
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (certification is null)
        {
            return CertificationDeleteResult.NotFound;
        }

        await mediaService.DeleteCertificationFilesAsync(id, cancellationToken);
        context.Certifications.Remove(certification);
        await context.SaveChangesAsync(cancellationToken);
        return CertificationDeleteResult.Deleted;
    }

    private static CertificationAdminDetails ToDetails(Certification certification)
    {
        var spanish = FindTranslation(certification, "es-ES");
        var english = FindTranslation(certification, "en-US");
        return new CertificationAdminDetails(
            certification.Id,
            certification.IssuedOn,
            certification.CredentialUrl,
            certification.CredentialId,
            certification.Hours,
            certification.ImagePath,
            certification.DisplayOrder,
            ToInput(spanish),
            ToInput(english),
            IsComplete(spanish),
            IsComplete(english));
    }

    private static CertificationTranslation? FindTranslation(Certification certification, string languageCode) =>
        certification.Translations.SingleOrDefault(translation => translation.LanguageCode == languageCode);

    private static bool IsComplete(CertificationTranslation? translation) =>
        translation is not null
        && !string.IsNullOrWhiteSpace(translation.Name)
        && !string.IsNullOrWhiteSpace(translation.Issuer);

    private static CertificationTranslationInput ToInput(CertificationTranslation? translation) =>
        new(translation?.Name ?? string.Empty, translation?.Issuer ?? string.Empty, translation?.Details);

    private static void UpdateTranslation(
        Certification certification,
        string languageCode,
        CertificationTranslationInput input)
    {
        var translation = FindTranslation(certification, languageCode);
        if (!IsCompleteInput(input))
        {
            if (translation is not null)
            {
                certification.Translations.Remove(translation);
            }

            return;
        }

        if (translation is null)
        {
            translation = new CertificationTranslation
            {
                CertificationId = certification.Id,
                LanguageCode = languageCode
            };
            certification.Translations.Add(translation);
        }

        translation.Name = input.Name.Trim();
        translation.Issuer = input.Issuer.Trim();
        translation.Details = NormalizeOptional(input.Details);
    }

    private static bool IsCompleteInput(CertificationTranslationInput input) =>
        !string.IsNullOrWhiteSpace(input.Name)
        && !string.IsNullOrWhiteSpace(input.Issuer);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
