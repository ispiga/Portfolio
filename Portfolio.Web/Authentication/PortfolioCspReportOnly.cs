namespace Portfolio.Web.Authentication;

public static class PortfolioCspReportOnly
{
    public const string Policy = "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' blob:; font-src 'self'; connect-src 'self'; worker-src 'self'; frame-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";

    public static IApplicationBuilder UsePortfolioCspReportOnly(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                if (context.Response.ContentType?.StartsWith("text/html", StringComparison.OrdinalIgnoreCase) == true
                    && !context.Response.Headers.ContainsKey("Content-Security-Policy-Report-Only"))
                {
                    var integrity = ImportMapCspIntegrity.GetIntegrity(context);
                    context.Response.Headers.Append(
                        "Content-Security-Policy-Report-Only",
                        ImportMapCspIntegrity.GetReportOnlyPolicy(integrity));
                }

                return Task.CompletedTask;
            });

            await next();
        });
    }
}
