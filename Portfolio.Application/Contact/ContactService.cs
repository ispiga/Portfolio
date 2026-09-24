using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Email;

namespace Portfolio.Application.Contact;

public sealed class ContactService(
    IEmailService emailService,
    IContactEmailConfiguration emailConfiguration,
    ILogger<ContactService> logger) : IContactService
{
    private static readonly EventId InvalidSubmissionEvent = new(1001, "InvalidSubmission");
    private static readonly EventId RejectedSubmissionEvent = new(1002, "RejectedSubmission");
    private static readonly EventId SubmissionSentEvent = new(1003, "SubmissionSent");
    private static readonly EventId SubmissionFailedEvent = new(1004, "SubmissionFailed");

    public async Task<ContactSubmissionResult> SubmitAsync(
        ContactRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!string.IsNullOrWhiteSpace(request.Honeypot))
        {
            logger.LogWarning(RejectedSubmissionEvent, "Contact submission rejected by anti-spam validation.");
            return ContactSubmissionResult.Rejected;
        }

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(request);
        if (!Validator.TryValidateObject(request, validationContext, validationResults, validateAllProperties: true))
        {
            logger.LogInformation(InvalidSubmissionEvent, "Contact submission failed server-side validation.");
            return ContactSubmissionResult.Invalid;
        }

        try
        {
            var emailMessage = new EmailMessage(
                emailConfiguration.FromAddress,
                emailConfiguration.ToAddress,
                request.Email.Trim(),
                emailConfiguration.Subject,
                $"Name: {request.Name.Trim()}\nEmail: {request.Email.Trim()}\n\n{request.Message.Trim()}");

            await emailService.SendAsync(emailMessage, cancellationToken);
            logger.LogInformation(SubmissionSentEvent, "Contact submission email sent.");
            return ContactSubmissionResult.Sent;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(SubmissionFailedEvent, exception, "Contact submission email failed.");
            return ContactSubmissionResult.Failed;
        }
    }
}
