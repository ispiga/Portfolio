namespace Portfolio.Application.Email;

public sealed record EmailMessage(
    string From,
    string To,
    string ReplyTo,
    string Subject,
    string TextBody);
