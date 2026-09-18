namespace Portfolio.Domain.Entities;

public sealed class Certification
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public DateOnly? IssuedOn { get; set; }

    public string? CredentialUrl { get; set; }

    public int DisplayOrder { get; set; }
}
