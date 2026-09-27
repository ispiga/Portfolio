using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Portfolio.Infrastructure.Identity;
using Portfolio.Web.Components.Admin;

namespace Portfolio.Web.Authentication;

public static class AdminAuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAdminAuthentication(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/admin/login/submit", LoginAsync)
            .AllowAnonymous();

        endpoints.MapPost("/admin/logout", LogoutAsync)
            .RequireAuthorization(PortfolioAuthorization.AdministratorPolicy);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        HttpContext context,
        IAntiforgery antiforgery,
        SignInManager<PortfolioUser> signInManager)
    {
        if (!await antiforgery.IsRequestValidAsync(context))
        {
            return Results.BadRequest();
        }

        var form = await context.Request.ReadFormAsync(context.RequestAborted);
        var input = new AdminLoginInput
        {
            Email = form[nameof(AdminLoginInput.Email)].ToString(),
            Password = form[nameof(AdminLoginInput.Password)].ToString(),
            ReturnUrl = form[nameof(AdminLoginInput.ReturnUrl)].ToString()
        };
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(input);

        if (!Validator.TryValidateObject(input, validationContext, validationResults, validateAllProperties: true))
        {
            var emailError = string.IsNullOrWhiteSpace(input.Email)
                ? "required"
                : new EmailAddressAttribute().IsValid(input.Email) ? null : "invalid";
            var passwordError = string.IsNullOrWhiteSpace(input.Password) ? "required" : null;
            var query = new List<string> { "error=validation" };

            if (emailError is not null)
            {
                query.Add($"emailError={emailError}");
            }

            if (passwordError is not null)
            {
                query.Add($"passwordError={passwordError}");
            }

            return Results.LocalRedirect($"/admin/login?{string.Join('&', query)}");
        }

        var result = await signInManager.PasswordSignInAsync(
            input.Email!,
            input.Password!,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Results.LocalRedirect("/admin/login?error=invalid");
        }

        return Results.LocalRedirect(IsLocalUrl(input.ReturnUrl) ? input.ReturnUrl! : "/admin");
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext context,
        IAntiforgery antiforgery,
        SignInManager<PortfolioUser> signInManager)
    {
        if (!await antiforgery.IsRequestValidAsync(context))
        {
            return Results.BadRequest();
        }

        await signInManager.SignOutAsync();
        return Results.LocalRedirect("/");
    }

    private static bool IsLocalUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl)
        && returnUrl[0] == '/'
        && (returnUrl.Length == 1 || returnUrl[1] is not ('/' or '\\'))
        && !returnUrl.Contains('\\', StringComparison.Ordinal);
}
