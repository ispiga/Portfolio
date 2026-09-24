using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Contact;
using Portfolio.Application.Email;
using Portfolio.Infrastructure.Email;
using Xunit;

namespace Portfolio.Tests;

public sealed class ContactServiceTests
{
    [Fact]
    public async Task Valid_request_is_sent_through_the_email_service()
    {
        var emailService = new FakeEmailService();
        var service = CreateService(emailService);
        var request = ValidRequest();

        var result = await service.SubmitAsync(request);

        Assert.Equal(ContactSubmissionResult.Sent, result);
        var message = Assert.Single(emailService.Messages);
        Assert.Equal("contacto@gmail.com", message.From);
        Assert.Equal("ispiga80@gmail.com", message.To);
        Assert.Equal(request.Email, message.ReplyTo);
        Assert.Contains(request.Message, message.TextBody);
    }

    [Fact]
    public void Required_fields_are_reported_by_data_annotations()
    {
        var request = new ContactRequest();

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(ContactRequest.Name)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(ContactRequest.Email)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(ContactRequest.Message)));
    }

    [Fact]
    public void Invalid_email_is_reported_by_data_annotations()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(ContactRequest.Email)));
    }

    [Fact]
    public void Maximum_lengths_are_enforced()
    {
        var request = ValidRequest();
        request.Name = new string('n', 101);
        request.Email = new string('e', 250) + "@x.com";
        request.Message = new string('m', 4001);

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(ContactRequest.Name)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(ContactRequest.Email)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(ContactRequest.Message)));
    }

    [Fact]
    public async Task Honeypot_submission_is_rejected_without_sending()
    {
        var emailService = new FakeEmailService();
        var service = CreateService(emailService);
        var request = ValidRequest();
        request.Honeypot = "automated value";

        var result = await service.SubmitAsync(request);

        Assert.Equal(ContactSubmissionResult.Rejected, result);
        Assert.Empty(emailService.Messages);
    }

    [Fact]
    public async Task Invalid_request_is_not_sent()
    {
        var emailService = new FakeEmailService();
        var service = CreateService(emailService);
        var request = ValidRequest();
        request.Message = string.Empty;

        var result = await service.SubmitAsync(request);

        Assert.Equal(ContactSubmissionResult.Invalid, result);
        Assert.Empty(emailService.Messages);
    }

    [Fact]
    public async Task Email_service_error_returns_failed_result()
    {
        var emailService = new FakeEmailService
        {
            Exception = new InvalidOperationException("SMTP unavailable")
        };
        var service = CreateService(emailService);

        var result = await service.SubmitAsync(ValidRequest());

        Assert.Equal(ContactSubmissionResult.Failed, result);
    }

    [Fact]
    public async Task Logs_do_not_contain_the_full_message_content()
    {
        var logger = new TestLogger<ContactService>();
        var emailService = new FakeEmailService
        {
            Exception = new InvalidOperationException("SMTP unavailable")
        };
        var service = CreateService(emailService, logger);
        var request = ValidRequest();

        await service.SubmitAsync(request);

        Assert.DoesNotContain(logger.Messages, message => message.Contains(request.Message, StringComparison.Ordinal));
    }

    [Fact]
    public void Invalid_smtp_options_are_rejected_by_data_annotations()
    {
        var options = new SmtpOptions();

        var errors = Validate(options);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(SmtpOptions.Host)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(SmtpOptions.FromAddress)));
        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(SmtpOptions.ToAddress)));
    }

    private static ContactService CreateService(
        FakeEmailService emailService,
        TestLogger<ContactService>? logger = null)
    {
        return new ContactService(
            emailService,
            new TestEmailConfiguration(),
            logger ?? new TestLogger<ContactService>());
    }

    private static ContactRequest ValidRequest() => new()
    {
        Name = "Ada Lovelace",
        Email = "ada@example.com",
        Message = "Me gustaría hablar sobre un proyecto."
    };

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            instance,
            new ValidationContext(instance),
            results,
            validateAllProperties: true);
        return results;
    }

    private sealed class FakeEmailService : IEmailService
    {
        public List<EmailMessage> Messages { get; } = [];
        public Exception? Exception { get; init; }

        public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            if (Exception is not null)
            {
                throw Exception;
            }

            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class TestEmailConfiguration : IContactEmailConfiguration
    {
        public string FromAddress => "contacto@gmail.com";
        public string ToAddress => "ispiga80@gmail.com";
        public string Subject => "Nuevo contacto";
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        void ILogger.Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();
            public void Dispose()
            {
            }
        }
    }
}
