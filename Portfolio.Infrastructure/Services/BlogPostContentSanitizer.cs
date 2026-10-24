using Ganss.Xss;
using Portfolio.Application.Blog;

namespace Portfolio.Infrastructure.Services;

public sealed class BlogPostContentSanitizer : IBlogPostContentSanitizer
{
    private static readonly string[] AllowedTags =
    [
        "a", "blockquote", "br", "code", "div", "em", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "img",
        "li", "ol", "p", "pre", "strong", "table", "tbody", "td", "tfoot", "th", "thead", "tr", "u", "ul"
    ];

    private static readonly string[] AllowedAttributes =
    [
        "alt", "border", "colspan", "height", "href", "rowspan", "src", "style", "target", "title", "width"
    ];

    private static readonly string[] AllowedCssProperties =
    ["border-collapse", "display", "margin-left", "margin-right", "text-align", "width"];

    public string Sanitize(string? content) => Sanitize(content, useAdministrativeImageUrls: false);

    public string SanitizeForAdministration(string? content) => Sanitize(content, useAdministrativeImageUrls: true);

    private static string Sanitize(string? content, bool useAdministrativeImageUrls)
    {
        if (string.IsNullOrEmpty(content))
        {
            return string.Empty;
        }

        var sanitizer = CreateSanitizer(useAdministrativeImageUrls);
        return sanitizer.Sanitize(content);
    }

    private static HtmlSanitizer CreateSanitizer(bool useAdministrativeImageUrls)
    {
        var sanitizer = new HtmlSanitizer
        {
            KeepChildNodes = true,
            AllowDataAttributes = false,
            AllowCssCustomProperties = false
        };
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.UnionWith(AllowedTags);
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.UnionWith(AllowedAttributes);
        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowedCssProperties.UnionWith(AllowedCssProperties);
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.UnionWith(["https", "http", "mailto"]);
        sanitizer.UriAttributes.Clear();
        sanitizer.UriAttributes.UnionWith(["href", "src"]);
        sanitizer.UriListAttributes.Clear();
        sanitizer.AllowedClasses.Clear();
        sanitizer.FilterUrl += (_, args) => FilterUrl(args, useAdministrativeImageUrls);
        sanitizer.PostProcessNode += (_, args) => PostProcessNode(args.Node);
        return sanitizer;
    }

    private static void FilterUrl(FilterUrlEventArgs args, bool useAdministrativeImageUrls)
    {
        if (args.Tag.LocalName.Equals("img", StringComparison.OrdinalIgnoreCase))
        {
            var id = GetBlogImageId(args.OriginalUrl);
            if (id is null)
            {
                args.SanitizedUrl = string.Empty;
                return;
            }

            args.SanitizedUrl = useAdministrativeImageUrls
                ? $"/admin/blog-post-images/{id:D}"
                : $"/blog-post-images/{id:D}";
            return;
        }

        if (!args.Tag.LocalName.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            args.SanitizedUrl = string.Empty;
            return;
        }

        var value = args.OriginalUrl.Trim();
        if (value.StartsWith("//", StringComparison.Ordinal)
            || value.Contains('\\')
            || value.Any(char.IsControl)
            || !Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri)
            || uri.IsAbsoluteUri && !AllowedLinkSchemes.Contains(uri.Scheme)
            || !uri.IsAbsoluteUri && !value.StartsWith('/') && !value.StartsWith('#'))
        {
            args.SanitizedUrl = string.Empty;
        }
    }

    private static readonly HashSet<string> AllowedLinkSchemes = new(StringComparer.OrdinalIgnoreCase)
    {
        "https", "http", "mailto"
    };

    private static Guid? GetBlogImageId(string url)
    {
        const string publicPrefix = "/blog-post-images/";
        const string adminPrefix = "/admin/blog-post-images/";
        var idText = url.StartsWith(publicPrefix, StringComparison.Ordinal)
            ? url[publicPrefix.Length..]
            : url.StartsWith(adminPrefix, StringComparison.Ordinal)
                ? url[adminPrefix.Length..]
                : string.Empty;
        return Guid.TryParseExact(idText, "D", out var id) ? id : null;
    }

    private static void PostProcessNode(AngleSharp.Dom.INode node)
    {
        if (node is not AngleSharp.Dom.IElement element)
        {
            return;
        }

        if (element.LocalName.Equals("img", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(element.GetAttribute("src")))
        {
            element.Parent?.RemoveChild(element);
            return;
        }

        if (!element.LocalName.Equals("a", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(element.GetAttribute("href")))
        {
            element.RemoveAttribute("href");
        }

        if (string.Equals(element.GetAttribute("target"), "_blank", StringComparison.OrdinalIgnoreCase))
        {
            element.SetAttribute("rel", "noopener noreferrer");
        }
        else
        {
            element.RemoveAttribute("rel");
            if (element.HasAttribute("target"))
            {
                element.RemoveAttribute("target");
            }
        }
    }
}