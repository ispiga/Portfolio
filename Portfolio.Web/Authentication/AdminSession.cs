using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Portfolio.Web.Authentication;

public static class AdminSession
{
    public const string ExpiresAtClaim = "portfolio.admin.session_expires_at";
    public static readonly TimeSpan Duration = TimeSpan.FromMinutes(30);

    public static void ConfigureCookie(CookieAuthenticationOptions options)
    {
        options.ExpireTimeSpan = Duration;
        options.SlidingExpiration = false;
        options.Events.OnSigningIn = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity
                && !identity.HasClaim(claim => claim.Type == ExpiresAtClaim))
            {
                var issuedAt = context.Properties?.IssuedUtc ?? DateTimeOffset.UtcNow;
                identity.AddClaim(CreateExpiryClaim(issuedAt, Duration));
            }

            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = context =>
        {
            if (context.Principal?.Identity?.IsAuthenticated == true
                && HasExpired(context.Principal, DateTimeOffset.UtcNow))
            {
                context.RejectPrincipal();
            }

            return Task.CompletedTask;
        };
    }

    public static Claim CreateExpiryClaim(DateTimeOffset issuedAt, TimeSpan duration) =>
        new(ExpiresAtClaim, (issuedAt + duration).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));

    public static bool HasExpired(ClaimsPrincipal principal, DateTimeOffset now)
    {
        var value = principal.FindFirst(ExpiresAtClaim)?.Value;
        return !long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expiresAt)
            || now.ToUnixTimeSeconds() >= expiresAt;
    }
}
