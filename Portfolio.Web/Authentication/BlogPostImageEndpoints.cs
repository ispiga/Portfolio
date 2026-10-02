using Portfolio.Application.Blog;
using Portfolio.Infrastructure.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Localization;

namespace Portfolio.Web.Authentication;

public static class BlogPostImageEndpoints
{
    public static IEndpointRouteBuilder MapBlogPostImageEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/blog-post-images/{imageId:guid}", async (
                Guid imageId,
                HttpContext context,
                IBlogPostImageService imageService,
                CancellationToken cancellationToken) =>
            {
                var image = await imageService.OpenImageAsync(imageId, administratorCanViewDrafts: false, cancellationToken);
                if (image is null)
                {
                    return Results.NotFound();
                }

                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; img-src 'self' data:; style-src 'unsafe-inline'; sandbox";
                return Results.File(image.Content, image.ContentType);
            })
            .AllowAnonymous();

        endpoints.MapGet("/admin/blog-post-images/{imageId:guid}", async (
                Guid imageId,
                HttpContext context,
                IBlogPostImageService imageService,
                CancellationToken cancellationToken) =>
            {
                var image = await imageService.OpenImageAsync(imageId, administratorCanViewDrafts: true, cancellationToken);
                if (image is null)
                {
                    return Results.NotFound();
                }

                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; img-src 'self' data:; style-src 'unsafe-inline'; sandbox";
                return Results.File(image.Content, image.ContentType);
            })
            .RequireAuthorization(PortfolioAuthorization.AdministratorPolicy);

        endpoints.MapPost("/admin/blog-post-images/{blogPostId:guid}", async (
                Guid blogPostId,
                HttpContext context,
                IAntiforgery antiforgery,
                IBlogPostImageService imageService,
                IStringLocalizer<SharedResource> localizer,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    await antiforgery.ValidateRequestAsync(context);
                }
                catch (AntiforgeryValidationException)
                {
                    return Results.BadRequest(new { error = localizer["BlogImageUploadFailed"].Value });
                }

                var requestLimit = checked(imageService.MaximumFileSizeBytes + 1024 * 1024);
                var bodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
                if (bodySizeFeature is { IsReadOnly: false })
                {
                    bodySizeFeature.MaxRequestBodySize = requestLimit;
                }

                var form = await context.Request.ReadFormAsync(cancellationToken);
                var file = form.Files.GetFile("file");
                if (file is null)
                {
                    return Results.BadRequest(new { error = localizer["BlogImageUploadFailed"].Value });
                }

                await using var stream = file.OpenReadStream();
                var result = await imageService.UploadImageAsync(
                    blogPostId,
                    file.FileName,
                    file.ContentType,
                    file.Length,
                    stream,
                    cancellationToken);
                if (!result.Succeeded || result.Image is null)
                {
                    return Results.BadRequest(new
                    {
                        error = localizer[result.Error switch
                        {
                            BlogPostImageError.UnsupportedType => "BlogImageUnsupportedType",
                            BlogPostImageError.TooLarge => "BlogImageTooLarge",
                            BlogPostImageError.InvalidContent => "BlogImageInvalidContent",
                            _ => "BlogImageUploadFailed"
                        }].Value
                    });
                }

                return Results.Ok(new { location = $"/admin/blog-post-images/{result.Image.Id}" });
            })
            .RequireAuthorization(PortfolioAuthorization.AdministratorPolicy);

        return endpoints;
    }
}
