using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace Portfolio.Web.Authentication;

public static class AdminSession
{
    public const string ExpiresAtClaim = "portfolio.admin.session_expires_at";
    public const string SessionIdClaim = "portfolio.admin.session_id";
    public const string ExpirationSignOutItem = "Portfolio.AdminSession.ExpirationSignOut";
    public static readonly TimeSpan Duration = TimeSpan.FromMinutes(15);

    public static void ConfigureCookie(CookieAuthenticationOptions options)
    {
        options.ExpireTimeSpan = Duration;
        options.SlidingExpiration = false;
        options.Events.OnSigningIn = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                if (!identity.HasClaim(claim => claim.Type == ExpiresAtClaim))
                {
                    var issuedAt = context.Properties?.IssuedUtc ?? DateTimeOffset.UtcNow;
                    identity.AddClaim(CreateExpiryClaim(issuedAt, Duration));
                    identity.AddClaim(new Claim(SessionIdClaim, Guid.NewGuid().ToString("N")));
                }

                if (TryGetSessionId(context.Principal, out var sessionId)
                    && long.TryParse(identity.FindFirst(ExpiresAtClaim)?.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expiry))
                {
                    context.HttpContext.RequestServices.GetRequiredService<AdminSessionActivityStore>()
                        .Start(sessionId, DateTimeOffset.FromUnixTimeSeconds(expiry));
                }
            }

            return Task.CompletedTask;
        };
        options.Events.OnValidatePrincipal = context =>
        {
            if (context.Principal?.Identity?.IsAuthenticated == true
                && HasExpired(context.Principal, DateTimeOffset.UtcNow, context.HttpContext.RequestServices.GetRequiredService<AdminSessionActivityStore>()))
            {
                context.RejectPrincipal();
            }

            return Task.CompletedTask;
        };
        options.Events.OnSigningOut = context =>
        {
            if (!context.HttpContext.Items.ContainsKey(ExpirationSignOutItem)
                && TryGetSessionId(context.HttpContext.User, out var sessionId))
            {
                context.HttpContext.RequestServices.GetRequiredService<AdminSessionActivityStore>().Remove(sessionId);
            }

            return Task.CompletedTask;
        };
    }

    public static Claim CreateExpiryClaim(DateTimeOffset issuedAt, TimeSpan duration) =>
        new(ExpiresAtClaim, (issuedAt + duration).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));

    public static bool HasExpired(ClaimsPrincipal principal, DateTimeOffset now)
        => HasExpired(principal, now, null);

    public static bool HasExpired(ClaimsPrincipal principal, DateTimeOffset now, AdminSessionActivityStore? activityStore)
    {
        var value = principal.FindFirst(ExpiresAtClaim)?.Value;
        if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expiresAt))
        {
            return true;
        }

        var fallback = DateTimeOffset.FromUnixTimeSeconds(expiresAt);
        var hasSessionId = TryGetSessionId(principal, out var sessionId);
        if (hasSessionId && activityStore is not null && !activityStore.TryGetExpiration(sessionId, out _))
        {
            return true;
        }

        var effectiveExpiration = hasSessionId && activityStore is not null
            ? activityStore.GetExpiration(sessionId, fallback)
            : fallback;
        return now >= effectiveExpiration;
    }

    public static bool TryGetSessionId(ClaimsPrincipal principal, out string sessionId)
    {
        sessionId = principal.FindFirst(SessionIdClaim)?.Value ?? string.Empty;
        return !string.IsNullOrWhiteSpace(sessionId);
    }

    public static async Task RenewAsync(HttpContext context, DateTimeOffset expiresAt)
    {
        var principal = context.User;
        if (!TryGetSessionId(principal, out var sessionId))
        {
            return;
        }

        var identity = (ClaimsIdentity)principal.Identity!;
        var currentExpiry = identity.FindFirst(ExpiresAtClaim);
        if (currentExpiry is not null)
        {
            identity.RemoveClaim(currentExpiry);
        }

        identity.AddClaim(CreateExpiryClaim(DateTimeOffset.UtcNow, Duration));
        var properties = new AuthenticationProperties
        {
            IsPersistent = false,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = expiresAt
        };
        await context.SignInAsync(IdentityConstants.ApplicationScheme, principal, properties);
    }
}
