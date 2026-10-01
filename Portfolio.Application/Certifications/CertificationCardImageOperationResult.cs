namespace Portfolio.Application.Certifications;

public sealed record CertificationCardImageOperationResult(CertificationAttachmentError Error)
{
    public bool Succeeded => Error == CertificationAttachmentError.None;
}
