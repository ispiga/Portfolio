using System.ComponentModel.DataAnnotations;

namespace Portfolio.Application.Contact;

public sealed class ContactRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(4000)]
    public string Message { get; set; } = string.Empty;

    [StringLength(200)]
    public string Honeypot { get; set; } = string.Empty;
}
