namespace Portfolio.Application.Email;

public interface IEmailService
{
    Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default);
}
