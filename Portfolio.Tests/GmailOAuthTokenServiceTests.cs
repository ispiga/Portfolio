using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Portfolio.Infrastructure.Email;
using Xunit;

namespace Portfolio.Tests;

public sealed class GmailOAuthTokenServiceTests
{
    [Fact]
    public async Task Refresh_token_is_exchanged_for_an_access_token_and_cached()
    {
        var handler = new TokenHandler();
        using var httpClient = new HttpClient(handler);
        var service = new GmailOAuthTokenService(httpClient, Options.Create(CreateOptions()));

        var firstToken = await service.GetAccessTokenAsync();
        var secondToken = await service.GetAccessTokenAsync();

        Assert.Equal("access-token", firstToken);
        Assert.Equal(firstToken, secondToken);
        Assert.Equal(1, handler.RequestCount);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
    }

    [Fact]
    public async Task Token_endpoint_failure_does_not_include_secret_values_in_the_exception()
    {
        var options = CreateOptions();
        var handler = new TokenHandler(HttpStatusCode.Unauthorized);
        using var httpClient = new HttpClient(handler);
        var service = new GmailOAuthTokenService(httpClient, Options.Create(options));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetAccessTokenAsync());

        Assert.DoesNotContain(options.OAuth.ClientSecret, exception.Message, StringComparison.Ordinal);
        Assert.DoesNotContain(options.OAuth.RefreshToken, exception.Message, StringComparison.Ordinal);
    }

    private static SmtpOptions CreateOptions() => new()
    {
        Host = "smtp.gmail.com",
        Port = 587,
        UserName = "sender@gmail.com",
        FromAddress = "sender@gmail.com",
        ToAddress = "recipient@example.com",
        Subject = "Contact",
        OAuth = new GmailOAuthOptions
        {
            ClientId = "client-id",
            ClientSecret = "client-secret",
            RefreshToken = "refresh-token"
        }
    };

    private sealed class TokenHandler(HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequest = request;

            var response = new HttpResponseMessage(statusCode);
            if (statusCode == HttpStatusCode.OK)
            {
                response.Content = JsonContent.Create(new
                {
                    access_token = "access-token",
                    expires_in = 3600
                });
            }

            return Task.FromResult(response);
        }
    }
}
