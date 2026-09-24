namespace Portfolio.Application.Contact;

public interface IContactEmailConfiguration
{
    string FromAddress { get; }
    string ToAddress { get; }
    string Subject { get; }
}
