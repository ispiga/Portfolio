using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Portfolio.Infrastructure.Storage;

internal static class SafeSvgDocument
{
    public static async Task<XDocument?> LoadValidatedAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 10 * 1024 * 1024,
                Async = true
            };
            using var reader = XmlReader.Create(stream, settings);
            var document = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
            return IsSafe(document) ? document : null;
        }
        catch (Exception exception) when (exception is XmlException or InvalidOperationException)
        {
            return null;
        }
    }

    public static async Task<bool> IsSafeFileAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await LoadValidatedAsync(stream, cancellationToken) is not null;
    }

    private static bool IsSafe(XDocument document)
    {
        var root = document.Root;
        if (root?.Name.LocalName != "svg" || root.Name.NamespaceName != "http://www.w3.org/2000/svg")
        {
            return false;
        }

        var blockedElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "script", "foreignObject", "iframe", "object", "embed", "audio", "video",
            "animate", "animateMotion", "animateTransform", "set", "discard"
        };
        foreach (var element in root.DescendantsAndSelf())
        {
            if (blockedElements.Contains(element.Name.LocalName)
                || element.Name.LocalName.Equals("style", StringComparison.OrdinalIgnoreCase)
                    && !IsSafeStyleElement(element.Value))
            {
                return false;
            }

            foreach (var attribute in element.Attributes())
            {
                if (attribute.IsNamespaceDeclaration)
                {
                    continue;
                }

                var name = attribute.Name.LocalName;
                var value = attribute.Value.Trim();
                var isRdfMetadataResource = name.Equals("resource", StringComparison.OrdinalIgnoreCase)
                    && attribute.Name.NamespaceName == "http://www.w3.org/1999/02/22-rdf-syntax-ns#"
                    && element.AncestorsAndSelf().Any(ancestor => ancestor.Name.LocalName == "metadata");
                if (name.StartsWith("on", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("base", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("href", StringComparison.OrdinalIgnoreCase) && !value.StartsWith('#')
                    || value.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
                    || value.Contains("data:", StringComparison.OrdinalIgnoreCase)
                    || value.Contains("file:", StringComparison.OrdinalIgnoreCase)
                    || !isRdfMetadataResource && value.Contains("http:", StringComparison.OrdinalIgnoreCase)
                    || !isRdfMetadataResource && value.Contains("https:", StringComparison.OrdinalIgnoreCase)
                    || value.Contains("@import", StringComparison.OrdinalIgnoreCase)
                    || value.Contains("expression(", StringComparison.OrdinalIgnoreCase)
                    || value.Contains("url(", StringComparison.OrdinalIgnoreCase)
                        && !Regex.IsMatch(
                            value,
                            "url\\(\\s*['\\\"]?#[-A-Za-z0-9_.]+['\\\"]?\\s*\\)",
                            RegexOptions.IgnoreCase))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool IsSafeStyleElement(string style)
    {
        if (style.Contains('\\')
            || style.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
            || style.Contains("data:", StringComparison.OrdinalIgnoreCase)
            || style.Contains("file:", StringComparison.OrdinalIgnoreCase)
            || style.Contains("http:", StringComparison.OrdinalIgnoreCase)
            || style.Contains("https:", StringComparison.OrdinalIgnoreCase)
            || style.Contains("@import", StringComparison.OrdinalIgnoreCase)
            || style.Contains("expression(", StringComparison.OrdinalIgnoreCase)
            || style.Contains("-moz-binding", StringComparison.OrdinalIgnoreCase)
            || style.Contains("behavior:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var localReferencesRemoved = Regex.Replace(
            style,
            @"url\(\s*(['""]?)#[-A-Za-z0-9_.:]+\1\s*\)",
            string.Empty,
            RegexOptions.IgnoreCase);
        return !localReferencesRemoved.Contains("url", StringComparison.OrdinalIgnoreCase);
    }
}
