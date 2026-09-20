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
            certification.IssuedOn,
            certification.CredentialUrl,
            certification.ImagePath,
            certification.DisplayOrder);
    }
}
