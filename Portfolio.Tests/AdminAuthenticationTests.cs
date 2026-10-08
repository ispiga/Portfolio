using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Portfolio.Application.Experiences;
using Portfolio.Application.Blog;
using Portfolio.Application.Certifications;
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
    private const string TestPassword = "C0mpl3x!Pass";
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
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapAdminAuthentication();
        app.MapExperienceAttachmentEndpoints();
        app.MapBlogPostImageEndpoints();
        app.MapCertificationMediaEndpoints();
        app.MapGet("/admin/login", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            var form = $"<input name=\"{tokens.FormFieldName}\" value=\"{WebUtility.HtmlEncode(tokens.RequestToken)}\" />";
            return Results.Content(form, "text/html");
        }).AllowAnonymous();
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
        Assert.Equal(5, identityOptions.Lockout.MaxFailedAccessAttempts);
        Assert.True((await authorization.AuthorizeAsync(administrator, null, PortfolioAuthorization.AdministratorPolicy)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(regularUser, null, PortfolioAuthorization.AdministratorPolicy)).Succeeded);
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
            throw new NotSupportedException();
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
