namespace Portfolio.Infrastructure.Email;

public interface IGmailOAuthTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
