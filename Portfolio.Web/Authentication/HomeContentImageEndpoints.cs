using Portfolio.Application.HomeContent;

namespace Portfolio.Web.Authentication;

public static class HomeContentImageEndpoints
{
    public static IEndpointRouteBuilder MapHomeContentImageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/home-content-images/{kind}/{entityId:guid}", async (
                string kind,
                Guid entityId,
                HttpContext context,
                IHomeContentImageStorageService imageStorage,
                CancellationToken cancellationToken) =>
            {
                var imageKind = kind switch
                {
                    "technology-logo" => HomeContentImageKind.TechnologyLogo,
                    "hobby" => HomeContentImageKind.Hobby,
                    "hero-profile" => HomeContentImageKind.HeroProfile,
                    _ => (HomeContentImageKind?)null
                };
                if (imageKind is null)
                {
                    return Results.NotFound();
                }

                var image = await imageStorage.OpenPublicReadAsync(imageKind.Value, entityId, cancellationToken);
                if (image is null)
                {
                    return Results.NotFound();
                }

                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                if (image.ContentType == "image/svg+xml")
                {
                    context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
                }

                return Results.File(image.Content, image.ContentType);
            })
            .AllowAnonymous();

        return endpoints;
    }
}