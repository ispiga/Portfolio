using Microsoft.AspNetCore.Routing;
using Portfolio.Application.Experiences;
using Portfolio.Infrastructure.Identity;

namespace Portfolio.Web.Authentication;

public static class ExperienceAttachmentEndpoints
{
    public static IEndpointRouteBuilder MapExperienceAttachmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/experience-attachments/{attachmentId:guid}", async (
                Guid attachmentId,
                HttpContext context,
                IExperienceAttachmentService attachmentService,
                CancellationToken cancellationToken) =>
            {
                var attachment = await attachmentService.OpenPublicReadAsync(attachmentId, cancellationToken);
                return attachment is null
                    ? Results.NotFound()
                    : CreateInlineFileResult(context, attachment);
            })
            .AllowAnonymous();

        endpoints.MapGet("/admin/experiences/attachments/{attachmentId:guid}", async (
                Guid attachmentId,
                HttpContext context,
                IExperienceAttachmentService attachmentService,
                CancellationToken cancellationToken) =>
            {
                var attachment = await attachmentService.OpenReadAsync(attachmentId, cancellationToken);
                if (attachment is null)
                {
                    return Results.NotFound();
                }

                return CreateInlineFileResult(context, attachment);
            })
            .RequireAuthorization(PortfolioAuthorization.AdministratorPolicy);

        return endpoints;
    }

    private static IResult CreateInlineFileResult(HttpContext context, ExperienceAttachmentContent attachment)
    {
        context.Response.Headers.CacheControl = "no-store";
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers.ContentDisposition = "inline";
        return Results.File(
            attachment.Content,
            attachment.Attachment.ContentType,
            enableRangeProcessing: true);
    }
}