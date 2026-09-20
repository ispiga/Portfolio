namespace Portfolio.Application.Certifications;

public sealed record CertificationReadModel(
    Guid Id,
    string Name,
    string Issuer,
    DateOnly? IssuedOn,
    string? CredentialUrl,
    string? ImagePath,
    int DisplayOrder);
