using Portfolio.Application.Certifications;

namespace Portfolio.Web.Authentication;

public static class CertificationMediaEndpoints
{
    public static IEndpointRouteBuilder MapCertificationMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/certification-attachments/{attachmentId:guid}", async (
                Guid attachmentId,
                HttpContext context,
                ICertificationMediaService mediaService,
                CancellationToken cancellationToken) =>
            {
                var attachment = await mediaService.OpenAttachmentAsync(attachmentId, cancellationToken);
                if (attachment is null)
                {
                    return Results.NotFound();
                }

                context.Response.Headers.CacheControl = "no-store";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers.ContentDisposition = "inline";
                return Results.File(attachment.Content, attachment.Attachment.ContentType, enableRangeProcessing: true);
            })
            .AllowAnonymous();

        endpoints.MapGet("/certification-card-images/{certificationId:guid}", async (
                Guid certificationId,
                HttpContext context,
                ICertificationMediaService mediaService,
                CancellationToken cancellationToken) =>
            {
                var image = await mediaService.OpenCardImageAsync(certificationId, cancellationToken);
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
