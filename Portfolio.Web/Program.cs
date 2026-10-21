using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Identity;
using Portfolio.Web.Authentication;
using Portfolio.Web.Components;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.AspNetCore.SignalR", LogLevel.Debug);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Http.Connections", LogLevel.Debug);
    builder.Logging.AddFilter("Microsoft.AspNetCore.Components.Server.Circuits", LogLevel.Debug);
}

builder.Services.AddLocalization();
builder.Services.AddPortfolioPersistence(builder.Configuration);
builder.Services.AddPortfolioIdentity();
builder.Services.AddRateLimiter(options => PortfolioRateLimiting.Configure(options));
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddMudServices();
builder.Services.AddSingleton<AdminSessionActivityStore>();
builder.Services.AddScoped<AdminSessionExpirationState>();
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddCookie(IdentityConstants.ApplicationScheme, options =>
    {
        options.Cookie.Name = "Portfolio.Admin.Authentication";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.LoginPath = "/admin/login";
        options.AccessDeniedPath = "/admin/access-denied";
        options.Cookie.MaxAge = AdminSession.Duration;
        AdminSession.ConfigureCookie(options);
    });

// Add services for modern Blazor Web App (Interactive Server)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<AuthenticationStateProvider, AdminAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UsePortfolioCspReportOnly();

var supportedCultures = new[] { "es-ES", "en-US" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es-ES")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true
        && context.User.IsInRole(PortfolioAuthorization.AdministratorRole)
        && context.Request.Path.StartsWithSegments("/admin")
        && context.Request.Path != "/admin/session/activity"
        && AdminSession.TryGetSessionId(context.User, out var sessionId))
    {
        var store = context.RequestServices.GetRequiredService<AdminSessionActivityStore>();
        var now = DateTimeOffset.UtcNow;
        if (!store.IsActive(sessionId, now))
        {
            context.Items[AdminSession.ExpirationSignOutItem] = true;
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (store.TryRenew(sessionId, now, TimeSpan.FromSeconds(30), out var expiresAt))
        {
            await AdminSession.RenewAsync(context, expiresAt);
        }
    }

    await next();
});

app.MapCultureSettingEndpoint(supportedCultures);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapAdminAuthentication();
app.MapContactSubmissionEndpoint();
app.MapExperienceAttachmentEndpoints();
app.MapProjectPreviewImageEndpoints();
app.MapCertificationMediaEndpoints();
app.MapBlogPostImageEndpoints();
app.MapHomeContentImageEndpoints();
app.MapVisualIdentityEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
