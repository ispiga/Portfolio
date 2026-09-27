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
    public async Task Login_cookie_contains_a_fixed_expiry_and_is_rejected_after_expiration()
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

        using var expired = await SendAsync(new HttpRequestMessage(HttpMethod.Get, "/admin"));
        Assert.Equal(HttpStatusCode.Redirect, expired.StatusCode);
        Assert.Equal("/admin/login", expired.Headers.Location?.AbsolutePath);
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
}
