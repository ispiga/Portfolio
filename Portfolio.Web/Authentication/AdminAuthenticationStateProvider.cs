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
    IOptions<IdentityOptions> identityOptions) : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(1);

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

        if (AdminSession.HasExpired(principal, DateTimeOffset.UtcNow))
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
