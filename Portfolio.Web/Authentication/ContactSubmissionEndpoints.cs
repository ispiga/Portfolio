using Portfolio.Application.Contact;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Features;

namespace Portfolio.Web.Authentication;

public static class ContactSubmissionEndpoints
{
    public static IEndpointRouteBuilder MapContactSubmissionEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/contact/submit", async (
                HttpContext context,
                IAntiforgery antiforgery,
                IContactService contactService,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    await antiforgery.ValidateRequestAsync(context);
                }
                catch (AntiforgeryValidationException)
                {
                    return Results.BadRequest();
                }

                var bodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
                if (bodySizeFeature is { IsReadOnly: false })
                {
                    bodySizeFeature.MaxRequestBodySize = 16 * 1024;
                }

                var request = await context.Request.ReadFromJsonAsync<ContactRequest>(cancellationToken);
                if (request is null)
                {
                    return Results.BadRequest();
                }

                var result = await contactService.SubmitAsync(request, cancellationToken);
                var resultName = result switch
                {
                    ContactSubmissionResult.Sent => "sent",
                    ContactSubmissionResult.Rejected => "rejected",
                    ContactSubmissionResult.Invalid => "invalid",
                    _ => "failed"
                };
                var statusCode = result == ContactSubmissionResult.Failed
                    ? StatusCodes.Status503ServiceUnavailable
                    : result == ContactSubmissionResult.Invalid
                        ? StatusCodes.Status422UnprocessableEntity
                        : StatusCodes.Status200OK;

                return Results.Json(new { result = resultName }, statusCode: statusCode);
            })
            .AllowAnonymous()
            .RequireRateLimiting(PortfolioRateLimiting.ContactSubmissionPolicy);

        return endpoints;
    }
}
