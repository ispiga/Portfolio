using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Portfolio.Web.Authentication;

public static class PortfolioRateLimiting
{
    public const string AdminLoginPolicy = "admin-login";
    public const string ContactSubmissionPolicy = "contact-submission";
    public const string BlogImageUploadPolicy = "blog-image-upload";

    public static void Configure(
        RateLimiterOptions options,
        TimeSpan? loginWindow = null,
        TimeSpan? contactWindow = null,
        TimeSpan? blogUploadWindow = null)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.OnRejected = async (context, _) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            }

            context.HttpContext.Response.ContentType = "application/problem+json";
            await context.HttpContext.Response.WriteAsJsonAsync(new
            {
                status = StatusCodes.Status429TooManyRequests,
                title = "Too many requests"
            });
        };

        AddIpPolicy(options, AdminLoginPolicy, 10, loginWindow ?? TimeSpan.FromMinutes(1));
        AddIpPolicy(options, ContactSubmissionPolicy, 3, contactWindow ?? TimeSpan.FromMinutes(10));
        AddIpPolicy(options, BlogImageUploadPolicy, 5, blogUploadWindow ?? TimeSpan.FromMinutes(1));
    }

    private static void AddIpPolicy(
        RateLimiterOptions options,
        string policyName,
        int permitLimit,
        TimeSpan window) =>
        options.AddPolicy(policyName, context => RateLimitPartition.GetFixedWindowLimiter(
            GetClientIp(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    private static string GetClientIp(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
