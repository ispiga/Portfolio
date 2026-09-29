using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Portfolio.Infrastructure.Identity;

namespace Portfolio.Web.Authentication;

internal sealed class AdminAuthenticationStateProvider(
    ILoggerFactory loggerFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<IdentityOptions> identityOptions,
    AdminSessionActivityStore activityStore,
    AdminSessionExpirationState expirationState) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(30);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var principal = authenticationState.User;
        if (principal.Identity?.IsAuthenticated != true)
        {
            return true;
        }

        if (AdminSession.TryGetSessionId(principal, out var sessionId)
            && activityStore.HasExpired(sessionId))
        {
            expirationState.MarkExpired();
            return false;
        }

        if (AdminSession.HasExpired(principal, DateTimeOffset.UtcNow, activityStore))
        {
            return false;
        }

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<PortfolioUser>>();
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return false;
        }

        if (!userManager.SupportsUserSecurityStamp)
        {
            return true;
        }

        var principalSecurityStamp = principal.FindFirstValue(
            identityOptions.Value.ClaimsIdentity.SecurityStampClaimType);
        if (string.IsNullOrWhiteSpace(principalSecurityStamp))
        {
            return false;
        }

        var currentSecurityStamp = await userManager.GetSecurityStampAsync(user);
        return string.Equals(principalSecurityStamp, currentSecurityStamp, StringComparison.Ordinal);
    }
}
