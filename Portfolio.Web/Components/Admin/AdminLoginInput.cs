using System.ComponentModel.DataAnnotations;

namespace Portfolio.Web.Components.Admin;

public sealed class AdminLoginInput
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [Required]
    public string? Password { get; set; }

    public string? ReturnUrl { get; set; }
}
