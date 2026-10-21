using Microsoft.AspNetCore.Localization;

namespace Portfolio.Web.Authentication;

public static class CultureEndpoints
{
    public static IEndpointRouteBuilder MapCultureSettingEndpoint(
        this IEndpointRouteBuilder endpoints,
        IReadOnlyCollection<string> supportedCultures)
    {
        endpoints.MapGet("/culture/set", (HttpContext context, string culture, string? redirectUri) =>
        {
            var supportedCulture = supportedCultures.FirstOrDefault(
                supported => string.Equals(supported, culture, StringComparison.OrdinalIgnoreCase));
            if (supportedCulture is null)
            {
                return Results.BadRequest();
            }

            context.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(supportedCulture)),
                new CookieOptions
                {
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });

            var localRedirectUri = IsSafeLocalRedirect(redirectUri) ? redirectUri! : "/";
            return Results.LocalRedirect(localRedirectUri);
        });

        return endpoints;
    }

    private static bool IsSafeLocalRedirect(string? redirectUri) =>
        !string.IsNullOrEmpty(redirectUri)
        && redirectUri.StartsWith("/", StringComparison.Ordinal)
        && !redirectUri.StartsWith("//", StringComparison.Ordinal)
        && !redirectUri.Contains('\\')
        && !redirectUri.Any(char.IsControl)
        && Uri.TryCreate(redirectUri, UriKind.Relative, out _);
}
