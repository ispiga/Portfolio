using System.ComponentModel.DataAnnotations;
using Portfolio.Application.Contact;

namespace Portfolio.Infrastructure.Email;

public sealed class SmtpOptions : IContactEmailConfiguration
{
    public const string SectionName = "ContactEmail";

    [Required]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; }

    [Required]
    [EmailAddress]
    public string UserName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string FromAddress { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ToAddress { get; init; } = string.Empty;

    [Required]
    public string Subject { get; init; } = string.Empty;

    public bool UseStartTls { get; init; } = true;

    [Required]
    public GmailOAuthOptions OAuth { get; init; } = new();
}

public sealed class GmailOAuthOptions
{
    public const string DefaultTokenEndpoint = "https://oauth2.googleapis.com/token";
    public const string DefaultScope = "https://mail.google.com/";

    [Required]
    public string TokenEndpoint { get; init; } = DefaultTokenEndpoint;

    [Required]
    public string ClientId { get; init; } = string.Empty;

    [Required]
    public string ClientSecret { get; init; } = string.Empty;

    [Required]
    public string RefreshToken { get; init; } = string.Empty;

    [Required]
    public string Scope { get; init; } = DefaultScope;
}
