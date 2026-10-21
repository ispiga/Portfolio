using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Portfolio.Application.Experiences;
using Portfolio.Application.Blog;
using Portfolio.Application.Certifications;
using Portfolio.Application.Contact;
using Portfolio.Application.Email;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Identity;
using Portfolio.Web.Authentication;
using Portfolio.Web.Components.Admin;
using Xunit;

namespace Portfolio.Tests;

public sealed class AdminAuthenticationTests : IAsyncLifetime
{
    private const string AdministratorEmail = "admin@example.test";
    private const string RegularUserEmail = "reader@example.test";
    private const string TestPassword = "C0mpl3x!Pass1";
    private readonly Dictionary<string, string> cookies = new(StringComparer.OrdinalIgnoreCase);
    private WebApplication app = null!;
    private HttpClient client = null!;
    private IDataProtector cookieProtector = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Development"
        });
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContextFactory<PortfolioDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        builder.Services.AddRateLimiter(options => PortfolioRateLimiting.Configure(
            options,
            loginWindow: TimeSpan.FromSeconds(30),
            contactWindow: TimeSpan.FromSeconds(2),
            blogUploadWindow: TimeSpan.FromSeconds(2)));
        builder.Services.AddAntiforgery(options =>
        {
            options.Cookie.Name = "Portfolio.Test.Antiforgery";
            options.Cookie.SecurePolicy = CookieSecurePolicy.None;
        });
        builder.Services.AddPortfolioIdentity();
        builder.Services.AddSingleton<AdminSessionActivityStore>();
        builder.Services.AddSingleton<IExperienceAttachmentService, TestExperienceAttachmentService>();
        builder.Services.AddSingleton<IBlogPostImageService, TestBlogPostImageService>();
        builder.Services.AddSingleton<ICertificationMediaService, TestCertificationAttachmentService>();
        builder.Services.AddSingleton<IContactService, TestContactService>();
        builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.Cookie.Name = "Portfolio.Test.Authentication";
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
                options.LoginPath = "/admin/login";
                options.AccessDeniedPath = "/admin/access-denied";
                AdminSession.ConfigureCookie(options);
            });

        app = builder.Build();
        app.Use(async (context, next) =>
        {
            if (IPAddress.TryParse(context.Request.Headers["X-Test-Client-IP"], out var testIp))
            {
                context.Connection.RemoteIpAddress = testIp;
            }

            await next();
        });
        app.UsePortfolioCspReportOnly();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.UseAntiforgery();
        app.MapAdminAuthentication();
        app.MapContactSubmissionEndpoint();
        app.MapCultureSettingEndpoint(["es-ES", "en-US"]);
        app.MapExperienceAttachmentEndpoints();
        app.MapBlogPostImageEndpoints();
        app.MapGet("/test/limited", () => Results.Ok())
            .RequireRateLimiting(PortfolioRateLimiting.ContactSubmissionPolicy);
        app.MapPost("/test/blog-image-upload/{blogPostId:guid}", (Guid blogPostId) => Results.Ok(blogPostId))
            .RequireRateLimiting(PortfolioRateLimiting.BlogImageUploadPolicy);
        app.MapGet("/test/static", () => Results.Ok("static-content"));
        app.MapCertificationMediaEndpoints();
        app.MapGet("/admin/login", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            var form = $"<input name=\"{tokens.FormFieldName}\" value=\"{WebUtility.HtmlEncode(tokens.RequestToken)}\" />";
            return Results.Content(form, "text/html");
        }).AllowAnonymous();
        app.MapGet("/test/csp-media", (HttpContext context) =>
        {
            context.Response.Headers["Content-Security-Policy"] = "default-src 'none'; sandbox";
            return Results.Content("<svg></svg>", "text/html");
        });
        var importMapDefinition = new ImportMapDefinition(
            new Dictionary<string, string> { ["test-module"] = "/test-module.js" },
            new Dictionary<string, IReadOnlyDictionary<string, string>>(),
            new Dictionary<string, string>());
        app.MapGet("/test/csp-import-map", (HttpContext context) =>
        {
            context.Response.Headers["Content-Security-Policy"] = "frame-ancestors 'self'";
            var integrity = ImportMapCspIntegrity.GetIntegrity(context);
            return Results.Content(
                $"<script type=\"importmap\" integrity=\"{integrity}\">{importMapDefinition}</script>",
                "text/html");
        }).WithMetadata(importMapDefinition);
        app.MapGet("/admin/access-denied", () => Results.Text("denied")).AllowAnonymous();
        app.MapGet("/admin", () => Results.Text("dashboard"))
            .RequireAuthorization(PortfolioAuthorization.AdministratorPolicy);
        await app.StartAsync();
        client = app.GetTestClient();
        var dataProtectionProvider = app.Services.GetRequiredService<IDataProtectionProvider>();
        cookieProtector = dataProtectionProvider.CreateProtector(
            "Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationMiddleware",
            IdentityConstants.ApplicationScheme,
            "v2");

        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await roleManager.CreateAsync(new IdentityRole(PortfolioAuthorization.AdministratorRole));
        await CreateUserAsync(AdministratorEmail, isAdministrator: true);
        await CreateUserAsync(RegularUserEmail, isAdministrator: false);
    }

    [Fact]
    public async Task Html_responses_include_report_only_policy_without_global_enforcement_and_preserve_specific_csp()
    {
        using var loginResponse = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/admin/login"));

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.Equal("text/html", loginResponse.Content.Headers.ContentType?.MediaType);
        Assert.Equal(PortfolioCspReportOnly.Policy, Assert.Single(loginResponse.Headers.GetValues("Content-Security-Policy-Report-Only")));
        Assert.False(loginResponse.Headers.Contains("Content-Security-Policy"));

        using var mediaResponse = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/test/csp-media"));

        Assert.Equal("default-src 'none'; sandbox", Assert.Single(mediaResponse.Headers.GetValues("Content-Security-Policy")));
        Assert.Equal(PortfolioCspReportOnly.Policy, Assert.Single(mediaResponse.Headers.GetValues("Content-Security-Policy-Report-Only")));
    }

    [Fact]
    public async Task Import_map_integrity_matches_serialized_definition_and_report_only_policy()
    {
        using var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/test/csp-import-map"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var mapJson = body[(body.IndexOf('>') + 1)..body.LastIndexOf("</script>", StringComparison.Ordinal)];
        var expectedIntegrity = $"sha256-{Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(mapJson.ReplaceLineEndings("\n"))))}";
        var reportOnlyPolicy = Assert.Single(response.Headers.GetValues("Content-Security-Policy-Report-Only"));

        Assert.Contains($"integrity=\"{expectedIntegrity}\"", body, StringComparison.Ordinal);
        Assert.Contains($"'{expectedIntegrity}'", reportOnlyPolicy, StringComparison.Ordinal);
        Assert.Contains("script-src 'self';", PortfolioCspReportOnly.Policy, StringComparison.Ordinal);
        Assert.Equal("frame-ancestors 'self'", Assert.Single(response.Headers.GetValues("Content-Security-Policy")));
    }

    [Fact]
    public async Task Certification_private_attachment_requires_administrator_and_public_route_hides_it()
    {
        using (var anonymousResponse = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/certifications/attachments/{TestCertificationAttachmentService.AttachmentId}")))
        {
            Assert.Equal(HttpStatusCode.Redirect, anonymousResponse.StatusCode);
            Assert.Equal("/admin/login", anonymousResponse.Headers.Location?.AbsolutePath);
        }

        using (var publicResponse = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/certification-attachments/{TestCertificationAttachmentService.AttachmentId}")))
        {
            Assert.Equal(HttpStatusCode.NotFound, publicResponse.StatusCode);
        }

        using var login = await LoginAsync(AdministratorEmail, TestPassword);
        using var adminResponse = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/certifications/attachments/{TestCertificationAttachmentService.AttachmentId}"));
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.Equal("no-store", adminResponse.Headers.CacheControl?.ToString());
    }

    [Fact]
    public async Task Blog_image_preview_requires_administrator_and_published_image_is_public()
    {
        var imageId = TestBlogPostImageService.ImageId;
        using (var anonymousResponse = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/blog-post-images/{imageId}")))
        {
            Assert.Equal(HttpStatusCode.Redirect, anonymousResponse.StatusCode);
            Assert.Equal("/admin/login", anonymousResponse.Headers.Location?.AbsolutePath);
        }

        using var login = await LoginAsync(AdministratorEmail, TestPassword);
        using var adminResponse = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/blog-post-images/{imageId}"));
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);
        Assert.Equal("no-store", adminResponse.Headers.CacheControl?.ToString());

        using var publicResponse = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/blog-post-images/{imageId}"));
        Assert.Equal(HttpStatusCode.OK, publicResponse.StatusCode);
        Assert.Equal("nosniff", Assert.Single(publicResponse.Headers.GetValues("X-Content-Type-Options")));
    }

    [Fact]
    public async Task Experience_attachment_download_requires_administrator_policy()
    {
        using var response = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/experiences/attachments/{TestExperienceAttachmentService.AttachmentId}"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/admin/login", response.Headers.Location?.AbsolutePath);
        Assert.Equal(
            PortfolioAuthorization.AdministratorPolicy,
            typeof(Experiences).GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal(
            PortfolioAuthorization.AdministratorPolicy,
            typeof(ExperienceEditor).GetCustomAttribute<AuthorizeAttribute>()?.Policy);
    }

    [Fact]
    public async Task Administrator_can_download_private_attachment_without_caching()
    {
        using var login = await LoginAsync(AdministratorEmail, TestPassword);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        using var response = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/experiences/attachments/{TestExperienceAttachmentService.AttachmentId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("%PDF-1.7", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Authenticated_non_administrator_cannot_download_private_attachment()
    {
        using var login = await LoginAsync(RegularUserEmail, TestPassword);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        using var response = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/admin/experiences/attachments/{TestExperienceAttachmentService.AttachmentId}"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/admin/access-denied", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Anonymous_visitor_can_view_public_attachment_inline()
    {
        using var response = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/experience-attachments/{TestExperienceAttachmentService.PublicAttachmentId}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("inline", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal("%PDF-1.7", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_view_private_attachment_through_public_route()
    {
        using var response = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            $"/experience-attachments/{TestExperienceAttachmentService.AttachmentId}"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_credentials_return_to_login_with_a_generic_error()
    {
        const string invalidPassword = "Incorrect!123";
        using var response = await LoginAsync(AdministratorEmail, invalidPassword);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/admin/login?", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Contains("error=invalid", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.DoesNotContain(invalidPassword, response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public void Admin_session_claim_expires_at_the_configured_session_end()
    {
        var issuedAt = DateTimeOffset.UtcNow;
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [AdminSession.CreateExpiryClaim(issuedAt, TimeSpan.FromMinutes(30))],
            authenticationType: "test"));

        Assert.False(AdminSession.HasExpired(principal, issuedAt.AddMinutes(29)));
        Assert.True(AdminSession.HasExpired(principal, issuedAt.AddMinutes(30)));
    }

    [Fact]
    public async Task Identity_registers_lockout_and_administrator_role_policy()
    {
        var identityOptions = app.Services.GetRequiredService<IOptions<IdentityOptions>>().Value;
        var authorization = app.Services.GetRequiredService<IAuthorizationService>();
        var administrator = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Role, PortfolioAuthorization.AdministratorRole)],
            authenticationType: "test"));
        var regularUser = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, RegularUserEmail)],
            authenticationType: "test"));

        Assert.True(identityOptions.User.RequireUniqueEmail);
        Assert.Equal(12, identityOptions.Password.RequiredLength);
        Assert.False(identityOptions.Password.RequireDigit);
        Assert.False(identityOptions.Password.RequireLowercase);
        Assert.False(identityOptions.Password.RequireUppercase);
        Assert.False(identityOptions.Password.RequireNonAlphanumeric);
        Assert.Equal(1, identityOptions.Password.RequiredUniqueChars);
        Assert.True(identityOptions.Lockout.AllowedForNewUsers);
        Assert.Equal(5, identityOptions.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), identityOptions.Lockout.DefaultLockoutTimeSpan);
        Assert.True((await authorization.AuthorizeAsync(administrator, null, PortfolioAuthorization.AdministratorPolicy)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(regularUser, null, PortfolioAuthorization.AdministratorPolicy)).Succeeded);
    }

    [Fact]
    public async Task Web_identity_accepts_long_password_without_composition_rules_and_rejects_short_password()
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PortfolioUser>>();

        var shortPasswordResult = await userManager.CreateAsync(
            new PortfolioUser { UserName = "short@example.test", Email = "short@example.test" },
            "abcdefghijk");
        var validPasswordResult = await userManager.CreateAsync(
            new PortfolioUser { UserName = "simple@example.test", Email = "simple@example.test" },
            "abcdefghijkl");

        Assert.False(shortPasswordResult.Succeeded);
        Assert.Contains(shortPasswordResult.Errors, error => error.Code == "PasswordTooShort");
        Assert.True(validPasswordResult.Succeeded, string.Join(", ", validPasswordResult.Errors.Select(error => error.Code)));
    }

    [Fact]
    public async Task Provisioning_identity_uses_the_same_password_policy_and_validation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContextFactory<PortfolioDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddPortfolioProvisioningIdentity();

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<IdentityOptions>>().Value;
        var userManager = provider.GetRequiredService<UserManager<PortfolioUser>>();

        Assert.Equal(12, options.Password.RequiredLength);
        Assert.False(options.Password.RequireDigit);
        Assert.False(options.Password.RequireLowercase);
        Assert.False(options.Password.RequireUppercase);
        Assert.False(options.Password.RequireNonAlphanumeric);
        Assert.Equal(1, options.Password.RequiredUniqueChars);
        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
        Assert.Equal(TimeSpan.FromMinutes(15), options.Lockout.DefaultLockoutTimeSpan);

        var shortPasswordResult = await userManager.CreateAsync(
            new PortfolioUser { UserName = "short@example.test", Email = "short@example.test" },
            "abcdefghijk");
        var validPasswordResult = await userManager.CreateAsync(
            new PortfolioUser { UserName = "simple@example.test", Email = "simple@example.test" },
            "abcdefghijkl");

        Assert.False(shortPasswordResult.Succeeded);
        Assert.Contains(shortPasswordResult.Errors, error => error.Code == "PasswordTooShort");
        Assert.True(validPasswordResult.Succeeded, string.Join(", ", validPasswordResult.Errors.Select(error => error.Code)));
    }

    [Fact]
    public async Task Five_failed_logins_lock_account_and_correct_password_does_not_bypass_lockout()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var failedLogin = await LoginAsync(AdministratorEmail, "incorrect-password");
            Assert.Equal(HttpStatusCode.Redirect, failedLogin.StatusCode);
            Assert.Contains("error=invalid", failedLogin.Headers.Location?.OriginalString, StringComparison.Ordinal);
        }

        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PortfolioUser>>();
        var administrator = await userManager.FindByEmailAsync(AdministratorEmail);
        Assert.NotNull(administrator);
        Assert.True(await userManager.IsLockedOutAsync(administrator));
        var lockoutEnd = await userManager.GetLockoutEndDateAsync(administrator);
        Assert.NotNull(lockoutEnd);
        Assert.InRange(lockoutEnd.Value, DateTimeOffset.UtcNow.AddMinutes(14), DateTimeOffset.UtcNow.AddMinutes(15));

        using var correctPasswordLogin = await LoginAsync(AdministratorEmail, TestPassword);
        Assert.Equal(HttpStatusCode.Redirect, correctPasswordLogin.StatusCode);
        Assert.Contains("error=invalid", correctPasswordLogin.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.True(await userManager.IsLockedOutAsync(administrator));
        Assert.Equal(lockoutEnd, await userManager.GetLockoutEndDateAsync(administrator));
    }

    [Fact]
    public async Task Login_rate_limit_returns_429_and_identity_lockout_remains_independent()
    {
        var token = await GetAntiforgeryTokenAsync();
        for (var attempt = 0; attempt < 10; attempt++)
        {
            using var response = await PostFormAsync("/admin/login/submit", new Dictionary<string, string>
            {
                [token.FieldName] = token.Value,
                [nameof(AdminLoginInput.Email)] = AdministratorEmail,
                [nameof(AdminLoginInput.Password)] = "incorrect-password"
            });
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        using var limited = await PostFormAsync("/admin/login/submit", new Dictionary<string, string>
        {
            [token.FieldName] = token.Value,
            [nameof(AdminLoginInput.Email)] = AdministratorEmail,
            [nameof(AdminLoginInput.Password)] = TestPassword
        });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Contains("Retry-After", limited.Headers.Select(header => header.Key), StringComparer.OrdinalIgnoreCase);

        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PortfolioUser>>();
        var administrator = await userManager.FindByEmailAsync(AdministratorEmail);
        Assert.NotNull(administrator);
        Assert.True(await userManager.IsLockedOutAsync(administrator));
    }

    [Fact]
    public async Task Contact_endpoint_enforces_quota_antiforgery_and_does_not_call_email_when_limited()
    {
        var token = await GetAntiforgeryTokenAsync();
        var contactService = app.Services.GetRequiredService<IContactService>() as TestContactService;
        Assert.NotNull(contactService);

        using (var missingToken = await SendContactAsync("10.0.0.9", null))
        {
            Assert.Equal(HttpStatusCode.BadRequest, missingToken.StatusCode);
            Assert.Equal(0, contactService.CallCount);
        }

        for (var submission = 0; submission < 3; submission++)
        {
            using var accepted = await SendContactAsync("10.0.0.1", token.Value);
            Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        }

        using var limited = await SendContactAsync("10.0.0.1", token.Value);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Contains("Retry-After", limited.Headers.Select(header => header.Key), StringComparer.OrdinalIgnoreCase);
        Assert.Equal(3, contactService.CallCount);

        using var differentIp = await SendContactAsync("10.0.0.2", token.Value);
        Assert.Equal(HttpStatusCode.OK, differentIp.StatusCode);
        Assert.Equal(4, contactService.CallCount);
    }

    [Fact]
    public async Task Contact_endpoint_preserves_honeypot_and_server_side_validation()
    {
        var token = await GetAntiforgeryTokenAsync();
        var contactService = app.Services.GetRequiredService<IContactService>() as TestContactService;
        Assert.NotNull(contactService);

        using var invalid = await SendContactAsync("10.0.0.3", token.Value, new ContactRequest
        {
            Name = "",
            Email = "invalid",
            Message = ""
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.StatusCode);
        Assert.Equal("invalid", (await invalid.Content.ReadFromJsonAsync<ContactResultPayload>())?.Result);
        Assert.Equal(1, contactService.CallCount);

        using var honeypot = await SendContactAsync("10.0.0.4", token.Value, new ContactRequest
        {
            Name = "Bot",
            Email = "bot@example.test",
            Message = "Automated message",
            Honeypot = "filled"
        });
        Assert.Equal(HttpStatusCode.OK, honeypot.StatusCode);
        Assert.Equal("rejected", (await honeypot.Content.ReadFromJsonAsync<ContactResultPayload>())?.Result);
        Assert.Equal(2, contactService.CallCount);
    }

    [Fact]
    public async Task Rate_limit_window_recovers_and_unlimited_routes_are_unaffected()
    {
        using (var first = await GetFromIpAsync("/test/limited", "10.0.0.5"))
        using (var second = await GetFromIpAsync("/test/limited", "10.0.0.5"))
        using (var third = await GetFromIpAsync("/test/limited", "10.0.0.5"))
        using (var fourth = await GetFromIpAsync("/test/limited", "10.0.0.5"))
        {
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
            Assert.Equal(HttpStatusCode.OK, second.StatusCode);
            Assert.Equal(HttpStatusCode.OK, third.StatusCode);
            Assert.Equal(HttpStatusCode.TooManyRequests, fourth.StatusCode);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(2200));
        using (var recovered = await GetFromIpAsync("/test/limited", "10.0.0.5"))
        {
            Assert.Equal(HttpStatusCode.OK, recovered.StatusCode);
        }

        using var unlimited = await GetFromIpAsync("/test/static", "10.0.0.5");
        Assert.Equal(HttpStatusCode.OK, unlimited.StatusCode);
    }

    [Fact]
    public async Task Blog_image_upload_is_limited_but_public_media_get_is_not()
    {
        var endpoints = app.Services.GetServices<EndpointDataSource>()
            .SelectMany(dataSource => dataSource.Endpoints)
            .OfType<RouteEndpoint>()
            .ToArray();
        var uploadEndpoint = Assert.Single(endpoints.Where(endpoint =>
            endpoint.RoutePattern.RawText == "/admin/blog-post-images/{blogPostId:guid}"
            && endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains(HttpMethods.Post) == true));
        Assert.Equal(
            PortfolioRateLimiting.BlogImageUploadPolicy,
            uploadEndpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName);

        var adminGetEndpoint = Assert.Single(endpoints.Where(endpoint =>
            endpoint.RoutePattern.RawText == "/admin/blog-post-images/{imageId:guid}"
            && endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Contains(HttpMethods.Get) == true));
        Assert.Null(adminGetEndpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>());

        using var login = await LoginAsync(AdministratorEmail, TestPassword);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        var token = await GetAntiforgeryTokenAsync();
        var blogPostId = Guid.NewGuid();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var upload = await SendLimitedUploadProbeAsync(blogPostId);
            Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        }

        using var limitedUpload = await SendLimitedUploadProbeAsync(blogPostId);
        Assert.Equal(HttpStatusCode.TooManyRequests, limitedUpload.StatusCode);
        Assert.Contains("Retry-After", limitedUpload.Headers.Select(header => header.Key), StringComparer.OrdinalIgnoreCase);

        using var publicImage = await GetFromIpAsync(
            $"/blog-post-images/{TestBlogPostImageService.ImageId}",
            "10.0.0.6");
        Assert.Equal(HttpStatusCode.OK, publicImage.StatusCode);

        using var adminPreview = await GetFromIpAsync(
            $"/admin/blog-post-images/{TestBlogPostImageService.ImageId}",
            "10.0.0.7");
        Assert.Equal(HttpStatusCode.OK, adminPreview.StatusCode);
    }

    [Fact]
    public async Task Existing_password_created_under_previous_policy_remains_valid_for_login()
    {
        const string existingPassword = "Legacy1234";
        using (var scope = app.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PortfolioUser>>();
            var administrator = await userManager.FindByEmailAsync(AdministratorEmail);
            Assert.NotNull(administrator);

            administrator.PasswordHash = userManager.PasswordHasher.HashPassword(administrator, existingPassword);
            var updateResult = await userManager.UpdateAsync(administrator);
            Assert.True(updateResult.Succeeded, string.Join(", ", updateResult.Errors.Select(error => error.Code)));
        }

        using var login = await LoginAsync(AdministratorEmail, existingPassword);

        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/admin", login.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Anonymous_dashboard_request_is_redirected_to_login()
    {
        using var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/admin"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/admin/login", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Authenticated_non_administrator_is_redirected_to_access_denied()
    {
        using var login = await LoginAsync(RegularUserEmail, TestPassword);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);

        using var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/admin"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/admin/access-denied", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Administrator_can_access_dashboard_and_logout_revokes_access()
    {
        using var login = await LoginAsync(AdministratorEmail, TestPassword);
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        Assert.Equal("/admin", login.Headers.Location?.OriginalString);

        using var dashboard = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/admin"));
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
        Assert.Equal("dashboard", await dashboard.Content.ReadAsStringAsync());

        var token = await GetAntiforgeryTokenAsync();
        using var logout = await PostFormAsync("/admin/logout", new Dictionary<string, string>
        {
            [token.FieldName] = token.Value
        });
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal("/", logout.Headers.Location?.OriginalString);

        using var afterLogout = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/admin"));
        Assert.Equal(HttpStatusCode.Redirect, afterLogout.StatusCode);
        Assert.Equal("/admin/login", afterLogout.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Login_cookie_uses_a_fifteen_minute_inactivity_expiry_and_is_rejected_after_expiration()
    {
        using var login = await LoginAsync(AdministratorEmail, TestPassword);
        var setCookie = Assert.Single(login.Headers.GetValues("Set-Cookie"));
        Assert.Contains("Portfolio.Test.Authentication", setCookie, StringComparison.Ordinal);

        var cookie = setCookie.Split(';', 2)[0];
        cookies["Portfolio.Test.Authentication"] = cookie[("Portfolio.Test.Authentication=".Length)..];
        using var authenticated = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/admin"));
        Assert.Equal(HttpStatusCode.OK, authenticated.StatusCode);

        var ticket = cookieProtector.Unprotect(WebEncoders.Base64UrlDecode(cookies["Portfolio.Test.Authentication"]));
        var authenticationTicket = TicketSerializer.Default.Deserialize(ticket);
        Assert.NotNull(authenticationTicket);
        var principal = authenticationTicket!.Principal;
        Assert.False(AdminSession.HasExpired(principal, DateTimeOffset.UtcNow));
        Assert.True(AdminSession.TryGetSessionId(principal, out var sessionId));
        Assert.Equal(TimeSpan.FromMinutes(15), AdminSession.Duration);

        var expiredIdentity = new ClaimsIdentity(principal.Identity);
        expiredIdentity.RemoveClaim(expiredIdentity.FindFirst(AdminSession.ExpiresAtClaim)!);
        expiredIdentity.AddClaim(new Claim(
            AdminSession.ExpiresAtClaim,
            DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds().ToString()));
        var expiredTicket = new AuthenticationTicket(
            new ClaimsPrincipal(expiredIdentity),
            authenticationTicket.Properties,
            IdentityConstants.ApplicationScheme);
        cookies["Portfolio.Test.Authentication"] = WebEncoders.Base64UrlEncode(
            cookieProtector.Protect(TicketSerializer.Default.Serialize(expiredTicket)));
        app.Services.GetRequiredService<AdminSessionActivityStore>().Remove(sessionId);

        using var expired = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/admin"));
        Assert.Equal(HttpStatusCode.Redirect, expired.StatusCode);
        Assert.Equal("/admin/login", expired.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Authenticated_activity_renews_expiry_but_respects_the_rate_limit()
    {
        using var login = await LoginAsync(AdministratorEmail, TestPassword);
        var cookie = Assert.Single(login.Headers.GetValues("Set-Cookie")).Split(';', 2)[0];
        cookies["Portfolio.Test.Authentication"] = cookie[("Portfolio.Test.Authentication=".Length)..];
        var ticket = cookieProtector.Unprotect(WebEncoders.Base64UrlDecode(cookies["Portfolio.Test.Authentication"]));
        var authenticationTicket = TicketSerializer.Default.Deserialize(ticket)!;
        var principal = authenticationTicket.Principal;
        Assert.True(AdminSession.TryGetSessionId(principal, out var sessionId));
        var store = app.Services.GetRequiredService<AdminSessionActivityStore>();
        var now = DateTimeOffset.UtcNow;
        store.Start(sessionId, now.AddMinutes(15));

        Assert.True(store.TryRenew(sessionId, now.AddSeconds(31), TimeSpan.FromSeconds(30), out var renewedExpiry));
        Assert.Equal(now.AddSeconds(31).AddMinutes(15), renewedExpiry);
        Assert.False(store.TryRenew(sessionId, now.AddSeconds(40), TimeSpan.FromSeconds(30), out _));
        Assert.False(AdminSession.HasExpired(principal, now.AddSeconds(32), store));
        Assert.True(AdminSession.HasExpired(principal, renewedExpiry, store));
    }

    [Fact]
    public void Expiration_state_is_not_marked_by_intentional_sign_out()
    {
        var state = new AdminSessionExpirationState();

        Assert.False(state.IsExpired);
        state.MarkExpired();
        state.MarkExpired();

        Assert.True(state.IsExpired);
    }

    [Fact]
    public async Task Login_model_validation_returns_localizable_field_error_codes()
    {
        var token = await GetAntiforgeryTokenAsync();
        using var response = await PostFormAsync("/admin/login/submit", new Dictionary<string, string>
        {
            [token.FieldName] = token.Value,
            [nameof(AdminLoginInput.Email)] = "not-an-email",
            [nameof(AdminLoginInput.Password)] = string.Empty
        });

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("error=validation", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Contains("emailError=invalid", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
        Assert.Contains("passwordError=required", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Culture_endpoint_rejects_unsupported_culture_and_safely_falls_back_for_malformed_redirect()
    {
        using (var unsupportedCulture = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            "/culture/set?culture=fr-FR&redirectUri=%2F")))
        {
            Assert.Equal(HttpStatusCode.BadRequest, unsupportedCulture.StatusCode);
        }

        using var malformedRedirect = await SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            "/culture/set?culture=en-US&redirectUri=%2F%5Cexample.test"));

        Assert.Equal(HttpStatusCode.Redirect, malformedRedirect.StatusCode);
        Assert.Equal("/", malformedRedirect.Headers.Location?.OriginalString);
        Assert.Contains(
            malformedRedirect.Headers.GetValues("Set-Cookie"),
            cookie => cookie.StartsWith(".AspNetCore.Culture=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Login_and_logout_reject_requests_without_antiforgery_token()
    {
        using var login = await PostFormAsync("/admin/login/submit", new Dictionary<string, string>
        {
            [nameof(AdminLoginInput.Email)] = AdministratorEmail,
            [nameof(AdminLoginInput.Password)] = TestPassword
        });
        Assert.Equal(HttpStatusCode.BadRequest, login.StatusCode);

        using var authenticated = await LoginAsync(AdministratorEmail, TestPassword);
        var response = await PostFormAsync("/admin/logout", new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        response.Dispose();

        using var activity = await PostFormAsync("/admin/session/activity", new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.BadRequest, activity.StatusCode);
    }

    [Fact]
    public void Dashboard_component_requires_administrator_policy_and_login_is_anonymous()
    {
        var authorization = Assert.Single(typeof(AdminDashboard).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true));
        Assert.Equal(PortfolioAuthorization.AdministratorPolicy, ((AuthorizeAttribute)authorization).Policy);
        Assert.NotEmpty(typeof(AdminLogin).GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true));
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await app.DisposeAsync();
    }

    private async Task CreateUserAsync(string email, bool isAdministrator)
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PortfolioUser>>();
        var user = new PortfolioUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(user, TestPassword);
        Assert.True(createResult.Succeeded, string.Join(", ", createResult.Errors.Select(error => error.Code)));

        if (isAdministrator)
        {
            var roleResult = await userManager.AddToRoleAsync(user, PortfolioAuthorization.AdministratorRole);
            Assert.True(roleResult.Succeeded, string.Join(", ", roleResult.Errors.Select(error => error.Code)));
        }
    }

    private sealed class TestBlogPostImageService : IBlogPostImageService
    {
        public static Guid ImageId { get; } = Guid.Parse("98615387-8cb9-4786-8e5a-52ebc184c15a");
        public long MaximumFileSizeBytes => 10 * 1024 * 1024;
        public Task<IReadOnlyList<BlogPostImageReadModel>> GetImagesAsync(Guid blogPostId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<BlogPostImageReadModel>>([]);
        public Task<BlogPostImageOperationResult> UploadImageAsync(Guid blogPostId, string fileName, string contentType, long fileSize, Stream content, CancellationToken cancellationToken = default) =>
            Task.FromResult(new BlogPostImageOperationResult(BlogPostImageError.InvalidContent));
        public Task<BlogPostImageOperationResult> SetFeaturedImageAsync(Guid imageId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<BlogPostImageError> DeleteImageAsync(Guid imageId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task CleanupBlogPostDirectoryAsync(Guid blogPostId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
        public Task<BlogPostImageContent?> OpenImageAsync(Guid imageId, bool administratorCanViewDrafts, CancellationToken cancellationToken = default) =>
            imageId == ImageId
                ? Task.FromResult<BlogPostImageContent?>(new(new MemoryStream([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]), "image/png"))
                : Task.FromResult<BlogPostImageContent?>(null);
    }

    private sealed class TestContactService : IContactService
    {
        public int CallCount { get; private set; }

        public Task<ContactSubmissionResult> SubmitAsync(
            ContactRequest request,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            var errors = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            var isValid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                request,
                new System.ComponentModel.DataAnnotations.ValidationContext(request),
                errors,
                validateAllProperties: true);
            if (!isValid)
            {
                return Task.FromResult(ContactSubmissionResult.Invalid);
            }

            return Task.FromResult(string.IsNullOrWhiteSpace(request.Honeypot)
                ? ContactSubmissionResult.Sent
                : ContactSubmissionResult.Rejected);
        }
    }

    private sealed record ContactResultPayload(string Result);

    private async Task<HttpResponseMessage> LoginAsync(string email, string password)
    {
        var token = await GetAntiforgeryTokenAsync();
        return await PostFormAsync("/admin/login/submit", new Dictionary<string, string>
        {
            [nameof(AdminLoginInput.Email)] = email,
            [nameof(AdminLoginInput.Password)] = password,
            [token.FieldName] = token.Value
        });
    }

    private async Task<HttpResponseMessage> SendContactAsync(
        string ipAddress,
        string? antiforgeryToken,
        ContactRequest? contactRequest = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/contact/submit")
        {
            Content = JsonContent.Create(contactRequest ?? new ContactRequest
            {
                Name = "Ada Lovelace",
                Email = "ada@example.test",
                Message = "Portfolio enquiry"
            })
        };
        request.Headers.TryAddWithoutValidation("X-Test-Client-IP", ipAddress);
        if (antiforgeryToken is not null)
        {
            request.Headers.TryAddWithoutValidation("RequestVerificationToken", antiforgeryToken);
        }

        return await SendAsync(request);
    }

    private async Task<HttpResponseMessage> SendLimitedUploadProbeAsync(Guid blogPostId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/test/blog-image-upload/{blogPostId}");
        request.Headers.TryAddWithoutValidation("X-Test-Client-IP", "10.0.0.7");
        return await SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetFromIpAsync(string path, string ipAddress)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation("X-Test-Client-IP", ipAddress);
        return await SendAsync(request);
    }

    private async Task<(string FieldName, string Value)> GetAntiforgeryTokenAsync()
    {
        using var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/admin/login"));
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "name=\\\"(?<name>[^\\\"]+)\\\" value=\\\"(?<value>[^\\\"]+)\\\"");
        Assert.True(match.Success, "The test login page did not render an antiforgery token.");
        return (match.Groups["name"].Value, WebUtility.HtmlDecode(match.Groups["value"].Value));
    }

    private async Task<HttpResponseMessage> PostFormAsync(string path, Dictionary<string, string> fields)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new FormUrlEncodedContent(fields)
        };
        return await SendAsync(request);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        if (cookies.Count > 0)
        {
            request.Headers.TryAddWithoutValidation(
                "Cookie",
                string.Join("; ", cookies.Select(cookie => $"{cookie.Key}={cookie.Value}")));
        }

        var response = await client.SendAsync(request);
        if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            foreach (var setCookie in setCookieHeaders)
            {
                var cookie = setCookie.Split(';', 2)[0];
                var separator = cookie.IndexOf('=');
                if (separator > 0)
                {
                    cookies[cookie[..separator]] = cookie[(separator + 1)..];
                }
            }
        }

        request.Dispose();
        return response;
    }

    private sealed class TestCertificationAttachmentService : ICertificationMediaService
    {
        public static Guid CertificationId { get; } = Guid.Parse("0b9ad134-3bcf-4e6c-a713-a5de9a5f66cd");
        public static Guid AttachmentId { get; } = Guid.Parse("91d2edee-849d-4f4d-b8f2-8d4dbb8d95f1");
        public long MaximumFileSizeBytes => 10 * 1024 * 1024;

        public Task<IReadOnlyList<CertificationAttachmentReadModel>> GetAttachmentsAsync(Guid certificationId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<CertificationAttachmentReadModel>>([]);
        public Task<CertificationAttachmentOperationResult> UploadAttachmentAsync(Guid certificationId, string fileName, string contentType, long fileSize, Stream content, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> UpdateAttachmentDisplayNameAsync(Guid attachmentId, string displayName, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> SetAttachmentPublicAsync(Guid attachmentId, bool isPublic, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> DeleteAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task<CertificationAttachmentContent?> OpenAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default) =>
            attachmentId == AttachmentId
                ? Task.FromResult<CertificationAttachmentContent?>(new(
                    new CertificationAttachmentReadModel(AttachmentId, CertificationId, "private.pdf", "private.pdf", "application/pdf", 8, DateTimeOffset.UtcNow, false),
                    new MemoryStream(System.Text.Encoding.ASCII.GetBytes("%PDF-1.7"))))
                : Task.FromResult<CertificationAttachmentContent?>(null);

        public Task<CertificationAttachmentContent?> OpenPublicAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default) =>
            Task.FromResult<CertificationAttachmentContent?>(null);

        public Task<CertificationCardImageOperationResult> UploadCardImageAsync(Guid certificationId, string fileName, string contentType, long fileSize, Stream content, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<bool> DeleteCardImageAsync(Guid certificationId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<CertificationImageContent?> OpenCardImageAsync(Guid certificationId, CancellationToken cancellationToken = default) => Task.FromResult<CertificationImageContent?>(null);
        public Task DeleteCertificationFilesAsync(Guid certificationId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CleanupCertificationDirectoryAsync(Guid certificationId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class TestExperienceAttachmentService : IExperienceAttachmentService
    {
        public static Guid AttachmentId { get; } = Guid.Parse("7a7e7cd2-c353-415d-968b-8e851341a5d9");
        public static Guid PublicAttachmentId { get; } = Guid.Parse("e41a5042-18b9-4c55-bf5e-b5744d6438e0");
        public int MaximumFileCount => 5;
        public long MaximumFileSizeBytes => 10 * 1024 * 1024;

        public Task<IReadOnlyList<ExperienceAttachmentReadModel>> GetAttachmentsAsync(
            Guid experienceId,
            CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ExperienceAttachmentReadModel>>([]);

        public Task<ExperienceAttachmentOperationResult> UploadAsync(
            Guid experienceId,
            string fileName,
            string contentType,
            long fileSize,
            Stream content,
            CancellationToken cancellationToken = default) => throw new NotSupportedException();

        public Task<bool> UpdateDisplayNameAsync(
            Guid attachmentId,
            string displayName,
            CancellationToken cancellationToken = default) => Task.FromResult(false);

        public Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task CleanupExperienceDirectoryAsync(Guid experienceId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<bool> SetPublicAsync(Guid attachmentId, bool isPublic, CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task<ExperienceAttachmentContent?> OpenReadAsync(
            Guid attachmentId,
            CancellationToken cancellationToken = default)
        {
            if (attachmentId != AttachmentId && attachmentId != PublicAttachmentId)
            {
                return Task.FromResult<ExperienceAttachmentContent?>(null);
            }

            var attachment = new ExperienceAttachmentReadModel(
                attachmentId,
                Guid.NewGuid(),
                "private.pdf",
                "private.pdf",
                "application/pdf",
                8,
                DateTimeOffset.UtcNow,
                attachmentId == PublicAttachmentId);
            return Task.FromResult<ExperienceAttachmentContent?>(new(
                attachment,
                new MemoryStream(System.Text.Encoding.ASCII.GetBytes("%PDF-1.7"))));
        }

        public Task<ExperienceAttachmentContent?> OpenPublicReadAsync(
            Guid attachmentId,
            CancellationToken cancellationToken = default) =>
            attachmentId == PublicAttachmentId
                ? OpenReadAsync(attachmentId, cancellationToken)
                : Task.FromResult<ExperienceAttachmentContent?>(null);
    }
}
