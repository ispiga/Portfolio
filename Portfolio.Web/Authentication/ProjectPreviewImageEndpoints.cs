using Portfolio.Application.Projects;

namespace Portfolio.Web.Authentication;

public static class ProjectPreviewImageEndpoints
{
    public static IEndpointRouteBuilder MapProjectPreviewImageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/project-preview-images/{projectId:guid}", async (
                Guid projectId,
                HttpContext context,
                IProjectPreviewImageService imageService,
                CancellationToken cancellationToken) =>
            {
                var image = await imageService.OpenPublicReadAsync(projectId, cancellationToken);
                if (image is null)
                {
                    return Results.NotFound();
                }

                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                return Results.File(image.Content, image.ContentType);
            })
            .AllowAnonymous();

        return endpoints;
    }
}
