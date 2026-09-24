namespace Portfolio.Application.Contact;

public interface IContactService
{
    Task<ContactSubmissionResult> SubmitAsync(
        ContactRequest request,
        CancellationToken cancellationToken = default);
}
