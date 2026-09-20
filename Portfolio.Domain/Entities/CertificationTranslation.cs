namespace Portfolio.Domain.Entities;

public sealed class CertificationTranslation
{
    public Guid CertificationId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public Certification Certification { get; set; } = null!;
}
