using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.VisualIdentity;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Services;
using Portfolio.Infrastructure.Storage;
using Portfolio.Web.Authentication;
using Xunit;

namespace Portfolio.Tests;

public sealed class VisualIdentityStylesheetTests
{
    [Fact]
    public async Task Stylesheet_serves_default_palette_values()
    {
        var identity = CreateIdentity(
            VisualIdentityValidator.DefaultPrimaryColor,
            VisualIdentityValidator.DefaultSecondaryColor,
            VisualIdentityValidator.DefaultAccentColor,
            Guid.Parse("00000000-0000-0000-0000-000000000001"));
        await using var host = await CreateHostAsync(identity);

        using var response = await host.Client.GetAsync(VisualIdentityEndpoints.ThemeStylesheetUrl(identity.AssetVersion));
        var css = await response.Content.ReadAsStringAsync();

        Assert.True(response.StatusCode == HttpStatusCode.OK, css);
        Assert.Equal("text/css", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType?.CharSet);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("public, max-age=31536000, immutable", response.Headers.CacheControl?.ToString());
        Assert.Contains($"--color-primary: {VisualIdentityValidator.DefaultPrimaryColor};", css, StringComparison.Ordinal);
        Assert.Contains($"--color-primary-contrast: {VisualIdentityValidator.ContrastingTextColor(VisualIdentityValidator.DefaultPrimaryColor)};", css, StringComparison.Ordinal);
        Assert.Equal(6, css.Split("--color-", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public async Task Stylesheet_uses_default_colors_and_contrasts_when_identity_colors_are_invalid()
    {
        var identity = CreateIdentity("invalid-primary", "invalid-secondary", "invalid-accent", Guid.NewGuid());
        await using var host = await CreateHostAsync(identity);

        using var response = await host.Client.GetAsync(VisualIdentityEndpoints.ThemeStylesheetUrl(identity.AssetVersion));
        var css = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"--color-primary: {VisualIdentityValidator.DefaultPrimaryColor};", css, StringComparison.Ordinal);
        Assert.Contains($"--color-secondary: {VisualIdentityValidator.DefaultSecondaryColor};", css, StringComparison.Ordinal);
        Assert.Contains($"--color-accent: {VisualIdentityValidator.DefaultAccentColor};", css, StringComparison.Ordinal);
        Assert.Contains($"--color-primary-contrast: {VisualIdentityValidator.ContrastingTextColor(VisualIdentityValidator.DefaultPrimaryColor)};", css, StringComparison.Ordinal);
        Assert.Contains($"--color-secondary-contrast: {VisualIdentityValidator.ContrastingTextColor(VisualIdentityValidator.DefaultSecondaryColor)};", css, StringComparison.Ordinal);
        Assert.Contains($"--color-accent-contrast: {VisualIdentityValidator.ContrastingTextColor(VisualIdentityValidator.DefaultAccentColor)};", css, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stylesheet_serves_custom_palette_and_contrasts()
    {
        var identity = CreateIdentity("#FFFFFF", "#000000", "#808080", Guid.NewGuid());
        await using var host = await CreateHostAsync(identity);

        using var response = await host.Client.GetAsync(VisualIdentityEndpoints.ThemeStylesheetUrl(identity.AssetVersion));
        var css = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("--color-primary: #FFFFFF;", css, StringComparison.Ordinal);
        Assert.Contains("--color-secondary: #000000;", css, StringComparison.Ordinal);
        Assert.Contains("--color-accent: #808080;", css, StringComparison.Ordinal);
        Assert.Contains($"--color-primary-contrast: {VisualIdentityValidator.ContrastingTextColor(identity.PrimaryColor)};", css, StringComparison.Ordinal);
        Assert.Contains($"--color-secondary-contrast: {VisualIdentityValidator.ContrastingTextColor(identity.SecondaryColor)};", css, StringComparison.Ordinal);
        Assert.Contains($"--color-accent-contrast: {VisualIdentityValidator.ContrastingTextColor(identity.AccentColor)};", css, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Stylesheet_redirects_stale_version_without_caching_redirect()
    {
        var current = CreateIdentity("#123456", "#654321", "#ABCDEF", Guid.NewGuid());
        await using var host = await CreateHostAsync(current);
        var oldVersion = Guid.NewGuid();

        using var response = await host.Client.GetAsync(VisualIdentityEndpoints.ThemeStylesheetUrl(oldVersion));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(VisualIdentityEndpoints.ThemeStylesheetUrl(current.AssetVersion), response.Headers.Location?.OriginalString);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Null(response.Content.Headers.ContentType);
    }

    [Fact]
    public async Task Stylesheet_rejects_invalid_version_route()
    {
        await using var host = await CreateHostAsync(CreateIdentity("#123456", "#654321", "#ABCDEF", Guid.NewGuid()));

        using var response = await host.Client.GetAsync("/visual-identity/theme/not-a-guid.css");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Null(response.Content.Headers.ContentType);
    }

    [Fact]
    public async Task Stylesheet_failure_is_not_returned_or_cached_as_css()
    {
        await using var host = await CreateHostAsync(new ThrowingVisualIdentityQueryService());

        using var response = await host.Client.GetAsync(VisualIdentityEndpoints.ThemeStylesheetUrl(Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Null(response.Content.Headers.ContentType);
    }

    [Fact]
    public async Task App_links_versioned_stylesheet_before_app_css_without_inline_identity_styles()
    {
        var appPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "Portfolio.Web", "Components", "App.razor"));
        var source = await File.ReadAllTextAsync(appPath);

        Assert.Contains("ThemeStylesheetUrl(identity.AssetVersion)", source, StringComparison.Ordinal);
        Assert.Contains("id=\"visual-identity-theme-stylesheet\"", source, StringComparison.Ordinal);
        Assert.True(source.IndexOf("ThemeStylesheetUrl(identity.AssetVersion)", StringComparison.Ordinal)
            < source.IndexOf("@Assets[\"css/app.css\"]", StringComparison.Ordinal));
        Assert.DoesNotContain("<style>", source, StringComparison.Ordinal);
        Assert.DoesNotContain("--color-primary:", source, StringComparison.Ordinal);
        Assert.Contains("<ImportMap integrity=\"@importMapIntegrity\" />", source, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Visual_identity_editor_updates_stylesheet_with_reloaded_version_after_successful_save()
    {
        var editorPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "Portfolio.Web", "Components", "Admin", "VisualIdentityEditor.razor"));
        var modulePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "Portfolio.Web", "wwwroot", "js", "visual-identity-theme.js"));
        var editorSource = await File.ReadAllTextAsync(editorPath);
        var moduleSource = await File.ReadAllTextAsync(modulePath);
        var brandImagePath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..",
            "Portfolio.Web", "Components", "Shared", "PortfolioBrandImage.razor"));
        var brandImageSource = await File.ReadAllTextAsync(brandImagePath);
        var saveMethodStart = editorSource.IndexOf("private async Task SaveAsync()", StringComparison.Ordinal);
        var saveMethodEnd = editorSource.IndexOf("private Task UploadMainLogoAsync", saveMethodStart, StringComparison.Ordinal);
        var saveMethod = editorSource[saveMethodStart..saveMethodEnd];

        Assert.True(saveMethod.IndexOf("if (!result.Succeeded)", StringComparison.Ordinal)
            < saveMethod.IndexOf("UpdateThemeStylesheetAsync", StringComparison.Ordinal));
        Assert.True(saveMethod.IndexOf("await LoadAsync()", StringComparison.Ordinal)
            < saveMethod.IndexOf("UpdateThemeStylesheetAsync", StringComparison.Ordinal));
        Assert.Contains("ThemeStylesheetUrl(currentAssetVersion.Value)", editorSource, StringComparison.Ordinal);
        Assert.Contains("currentSettings!.MainLogoUrl", editorSource, StringComparison.Ordinal);
        Assert.Contains("currentSettings.FooterLogoUrl", editorSource, StringComparison.Ordinal);
        Assert.Contains("currentAssetVersion == previousAssetVersion", editorSource, StringComparison.Ordinal);
        Assert.Contains("stylesheet.href !== resolvedHref", moduleSource, StringComparison.Ordinal);
        Assert.Contains("stylesheet.href = resolvedHref", moduleSource, StringComparison.Ordinal);
        Assert.Contains("data-visual-identity-asset=\"@AssetName\"", brandImageSource, StringComparison.Ordinal);
        Assert.Contains("image.src !== resolvedUrl", moduleSource, StringComparison.Ordinal);
        Assert.Contains("image.src = resolvedUrl", moduleSource, StringComparison.Ordinal);
        Assert.Contains("updateLogo(\"mainlogo\", mainLogoUrl)", moduleSource, StringComparison.Ordinal);
        Assert.Contains("updateLogo(\"footerlogo\", footerLogoUrl)", moduleSource, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Saving_unchanged_palette_keeps_version_and_changed_palette_rotates_it()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var factory = new TestContextFactory(databaseName);
        var initialVersion = Guid.NewGuid();
        await using (var context = factory.CreateDbContext())
        {
            context.VisualIdentitySettings.Add(new VisualIdentitySettings
            {
                Id = VisualIdentitySettings.SingletonId,
                PrimaryColor = "#00A3FF",
                SecondaryColor = "#3E00D1",
                AccentColor = "#C98F42",
                AssetVersion = initialVersion
            });
            await context.SaveChangesAsync();
        }

        var service = new VisualIdentitySettingsService(factory, new TestAssetStorageService());
        var unchanged = await service.SaveAsync(new("#00a3ff", "#3e00d1", "#c98f42", true, null, null));
        var afterUnchanged = await service.GetAsync();
        var changed = await service.SaveAsync(new("#112233", "#445566", "#778899", true, null, null));
        var afterChanged = await service.GetAsync();

        Assert.True(unchanged.Succeeded);
        Assert.Equal(initialVersion, afterUnchanged.AssetVersion);
        Assert.True(changed.Succeeded);
        Assert.NotEqual(initialVersion, afterChanged.AssetVersion);
        Assert.Equal("#112233", afterChanged.PrimaryColor);
    }

    private static VisualIdentitySettingsModel CreateIdentity(string primary, string secondary, string accent, Guid version) => new(
        primary,
        secondary,
        accent,
        true,
        null,
        null,
        true,
        "/logo.svg",
        null,
        VisualIdentityVariantStatus.Automatic,
        false,
        "/favicon.svg",
        "image/svg+xml",
        VisualIdentityVariantStatus.Automatic,
        false,
        version);

    private static async Task<TestHost> CreateHostAsync(VisualIdentitySettingsModel identity) =>
        await CreateHostAsync(new TestVisualIdentityQueryService(identity));

    private static async Task<TestHost> CreateHostAsync(IVisualIdentityQueryService service)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<IVisualIdentityQueryService>(service);
        builder.Services.AddScoped<IVisualIdentityAdministrationService>(_ => null!);
        var app = builder.Build();
        var server = (TestServer)app.Services.GetRequiredService<IServer>();
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception exception)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(exception.ToString());
            }
        });
        app.UseRouting();
        app.MapVisualIdentityEndpoints();
        await app.StartAsync();
        return new TestHost(app, server);
    }

    private sealed class TestHost(WebApplication app, TestServer server) : IAsyncDisposable
    {
        public HttpClient Client { get; } = server.CreateClient();

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await app.DisposeAsync();
        }
    }

    private sealed class TestVisualIdentityQueryService(VisualIdentitySettingsModel identity) : IVisualIdentityQueryService
    {
        public Task<VisualIdentitySettingsModel> GetAsync(CancellationToken cancellationToken = default) => Task.FromResult(identity);

        public Task<VisualIdentityAssetContent?> OpenAssetAsync(
            VisualIdentityAssetKind kind,
            Guid assetVersion,
            CancellationToken cancellationToken = default) => Task.FromResult<VisualIdentityAssetContent?>(null);
    }

    private sealed class ThrowingVisualIdentityQueryService : IVisualIdentityQueryService
    {
        public Task<VisualIdentitySettingsModel> GetAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Test identity query failure.");

        public Task<VisualIdentityAssetContent?> OpenAssetAsync(
            VisualIdentityAssetKind kind,
            Guid assetVersion,
            CancellationToken cancellationToken = default) => Task.FromResult<VisualIdentityAssetContent?>(null);
    }

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }

    private sealed class TestAssetStorageService : IVisualIdentityAssetStorageService
    {
        public long MaximumFileSizeBytes => VisualIdentityValidator.MaximumAssetSizeBytes;

        public Task<VisualIdentityAssetUploadResult> UploadAsync(
            VisualIdentityAssetKind kind,
            string fileName,
            string contentType,
            long fileSize,
            Stream content,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task DeleteAsync(string? storageKey) => Task.CompletedTask;

        public Task<VisualIdentityAssetContent?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default) =>
            Task.FromResult<VisualIdentityAssetContent?>(null);

        public Task<VisualIdentityAssetContent> OpenDefaultLogoAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<VisualIdentityAssetContent> OpenDefaultFaviconAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
