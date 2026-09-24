using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Portfolio.Application.Email;

namespace Portfolio.Infrastructure.Email;

public sealed class MailKitEmailService(
    IOptions<SmtpOptions> options,
    IGmailOAuthTokenService tokenService) : IEmailService
{
    private readonly SmtpOptions options = options.Value;

    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var mimeMessage = new MimeMessage
        {
            Subject = message.Subject
        };
        mimeMessage.From.Add(MailboxAddress.Parse(message.From));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.ReplyTo.Add(MailboxAddress.Parse(message.ReplyTo));
        mimeMessage.Body = new TextPart("plain")
        {
            Text = message.TextBody
        };

        using var client = new SmtpClient();
        var security = options.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await client.ConnectAsync(options.Host, options.Port, security, cancellationToken);
        var accessToken = await tokenService.GetAccessTokenAsync(cancellationToken);
        await client.AuthenticateAsync(
            new SaslMechanismOAuth2(options.UserName, accessToken),
            cancellationToken);
        await client.SendAsync(mimeMessage, cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}
