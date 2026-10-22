using System.Text;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.VisualIdentity;
using Portfolio.Infrastructure.Identity;

namespace Portfolio.Web.Authentication;

public static class VisualIdentityEndpoints
{
    public static string ThemeStylesheetUrl(Guid assetVersion) =>
        $"/visual-identity/theme/{assetVersion:N}.css";

    public static IEndpointRouteBuilder MapVisualIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/visual-identity/theme/{assetVersion:guid}.css", async (
                Guid assetVersion,
                HttpContext context,
                [FromServices] IVisualIdentityQueryService visualIdentity,
                CancellationToken cancellationToken) =>
            {
                context.Response.Headers.CacheControl = "no-store";
                var identity = await visualIdentity.GetAsync(cancellationToken);
                if (identity.AssetVersion != assetVersion)
                {
                    return Results.Redirect(ThemeStylesheetUrl(identity.AssetVersion));
                }

                var primaryColor = SafeColor(identity.PrimaryColor, VisualIdentityValidator.DefaultPrimaryColor);
                var secondaryColor = SafeColor(identity.SecondaryColor, VisualIdentityValidator.DefaultSecondaryColor);
                var accentColor = SafeColor(identity.AccentColor, VisualIdentityValidator.DefaultAccentColor);
                var css = $$"""
                    html:root {
                        --color-primary: {{primaryColor}};
                        --color-secondary: {{secondaryColor}};
                        --color-accent: {{accentColor}};
                        --color-primary-contrast: {{VisualIdentityValidator.ContrastingTextColor(primaryColor)}};
                        --color-secondary-contrast: {{VisualIdentityValidator.ContrastingTextColor(secondaryColor)}};
                        --color-accent-contrast: {{VisualIdentityValidator.ContrastingTextColor(accentColor)}};
                    }
                    """;

                context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                return Results.Text(css, "text/css; charset=utf-8", Encoding.UTF8);
            })
            .AllowAnonymous();

        endpoints.MapGet("/visual-identity/assets/{kind}/{assetVersion:guid}", async (
                string kind,
                Guid assetVersion,
                HttpContext context,
                IVisualIdentityQueryService visualIdentity,
                CancellationToken cancellationToken) =>
            {
                var assetKind = kind switch
                {
                    "mainlogo" => VisualIdentityAssetKind.MainLogo,
                    "footerlogo" => VisualIdentityAssetKind.FooterLogo,
                    "favicon" => VisualIdentityAssetKind.Favicon,
                    _ => (VisualIdentityAssetKind?)null
                };
                if (assetKind is null)
                {
                    return Results.NotFound();
                }

                var asset = await visualIdentity.OpenAssetAsync(assetKind.Value, assetVersion, cancellationToken);
                if (asset is null)
                {
                    return Results.NotFound();
                }

                context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                if (asset.ContentType == "image/svg+xml")
                {
                    context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
                }

                return Results.File(asset.Content, asset.ContentType);
            })
            .AllowAnonymous();

        endpoints.MapGet("/admin/visual-identity/preview/{kind}", async (
                string kind,
                string primaryColor,
                string secondaryColor,
                string accentColor,
                bool logoColorsLinkedToPalette,
                string? logoColorStartOverride,
                string? logoColorEndOverride,
                HttpContext context,
                IVisualIdentityAdministrationService visualIdentity,
                CancellationToken cancellationToken) =>
            {
                var assetKind = kind switch
                {
                    "mainlogo" => VisualIdentityAssetKind.MainLogo,
                    "footerlogo" => VisualIdentityAssetKind.FooterLogo,
                    "favicon" => VisualIdentityAssetKind.Favicon,
                    _ => (VisualIdentityAssetKind?)null
                };
                if (assetKind is null)
                {
                    return Results.NotFound();
                }

                var request = new VisualIdentityEditRequest(
                    primaryColor,
                    secondaryColor,
                    accentColor,
                    logoColorsLinkedToPalette,
                    logoColorStartOverride,
                    logoColorEndOverride);
                var asset = await visualIdentity.OpenPreviewAssetAsync(assetKind.Value, request, cancellationToken);
                if (asset is null)
                {
                    return Results.NotFound();
                }

                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                if (asset.ContentType == "image/svg+xml")
                {
                    context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
                }

                return Results.File(asset.Content, asset.ContentType);
            })
            .RequireAuthorization(PortfolioAuthorization.AdministratorPolicy);

        return endpoints;
    }

    private static string SafeColor(string color, string fallback) =>
        VisualIdentityValidator.IsHexColor(color) ? color : fallback;
}
