using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Portfolio.Infrastructure.Email;

public sealed class GmailOAuthTokenService(
    HttpClient httpClient,
    IOptions<SmtpOptions> options) : IGmailOAuthTokenService
{
    private readonly SmtpOptions options = options.Value;
    private readonly SemaphoreSlim tokenLock = new(1, 1);
    private string? accessToken;
    private DateTimeOffset accessTokenExpiresAt;

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (HasUsableToken())
        {
            return accessToken!;
        }

        await tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (HasUsableToken())
            {
                return accessToken!;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, options.OAuth.TokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["client_id"] = options.OAuth.ClientId,
                    ["client_secret"] = options.OAuth.ClientSecret,
                    ["refresh_token"] = options.OAuth.RefreshToken,
                    ["grant_type"] = "refresh_token",
                    ["scope"] = options.OAuth.Scope
                })
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"The Gmail OAuth token endpoint returned HTTP {(int)response.StatusCode}.");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<GmailTokenResponse>(cancellationToken);
            if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException("The Gmail OAuth token endpoint returned no access token.");
            }

            accessToken = tokenResponse.AccessToken;
            var lifetime = Math.Max(tokenResponse.ExpiresIn, 60);
            accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(lifetime - 60);
            return accessToken;
        }
        finally
        {
            tokenLock.Release();
        }
    }

    private bool HasUsableToken() =>
        !string.IsNullOrWhiteSpace(accessToken)
        && accessTokenExpiresAt > DateTimeOffset.UtcNow;

    private sealed class GmailTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }
}
