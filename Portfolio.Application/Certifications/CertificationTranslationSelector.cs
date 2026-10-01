using Portfolio.Domain.Entities;

namespace Portfolio.Application.Certifications;

public static class CertificationTranslationSelector
{
    public static CertificationReadModel? Select(Certification certification, string cultureName)
    {
        var culture = string.Equals(cultureName, "en-US", StringComparison.OrdinalIgnoreCase)
            ? "en-US"
            : "es-ES";

        var translation = certification.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, culture, StringComparison.OrdinalIgnoreCase))
            ?? certification.Translations.FirstOrDefault(candidate =>
                string.Equals(candidate.LanguageCode, "es-ES", StringComparison.OrdinalIgnoreCase));

        if (translation is null
            || string.IsNullOrWhiteSpace(translation.Name)
            || string.IsNullOrWhiteSpace(translation.Issuer))
        {
            return null;
        }

        return new CertificationReadModel(
            certification.Id,
            translation.Name,
            translation.Issuer,
            string.IsNullOrWhiteSpace(translation.Details) ? null : translation.Details,
            certification.IssuedOn,
            certification.CredentialUrl,
            certification.CredentialId,
            certification.Hours,
            GetPublicImagePath(certification),
            certification.DisplayOrder,
            certification.Attachments
                .OrderBy(attachment => attachment.CreatedAt)
                .ThenBy(attachment => attachment.Id)
                .Select(attachment => new CertificationAttachmentReadModel(
                    attachment.Id,
                    attachment.CertificationId,
                    attachment.OriginalFileName,
                    attachment.DisplayName,
                    attachment.ContentType,
                    attachment.SizeBytes,
                    attachment.CreatedAt))
                .ToArray());
    }

    private static string? GetPublicImagePath(Certification certification)
    {
        if (string.IsNullOrWhiteSpace(certification.ImagePath)
            || !certification.ImagePath.StartsWith("certifications/", StringComparison.Ordinal))
        {
            return certification.ImagePath;
        }

        return $"/certification-card-images/{certification.Id}?v={Uri.EscapeDataString(Path.GetFileName(certification.ImagePath))}";
    }
}
