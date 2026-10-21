namespace Portfolio.Application.Blog;

public interface IBlogPostContentSanitizer
{
    string Sanitize(string? content);

    // Use only when returning content to the authenticated TinyMCE editor.
    string SanitizeForAdministration(string? content);
}