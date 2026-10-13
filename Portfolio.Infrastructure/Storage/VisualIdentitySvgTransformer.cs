using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Portfolio.Infrastructure.Storage;

internal static class VisualIdentitySvgTransformer
{
    private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";
    private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";
    private const string MainGradientId = "brandGradient";
    private const string ReverseGradientId = "brandGradientReverse";

    public static bool SupportsAutomaticRecoloring(XDocument document)
    {
        var root = document.Root;
        var mainGradient = FindById(root, MainGradientId);
        var reverseGradient = FindById(root, ReverseGradientId);
        if (root is null || mainGradient?.Name != Svg + "linearGradient" || reverseGradient?.Name != Svg + "linearGradient"
            || root.Descendants(Svg + "style").Any()
            || root.DescendantsAndSelf().Count(element => element.Attribute("id")?.Value == MainGradientId) != 1
            || root.DescendantsAndSelf().Count(element => element.Attribute("id")?.Value == ReverseGradientId) != 1)
        {
            return false;
        }

        var stops = mainGradient.Elements(Svg + "stop").ToArray();
        return stops.Length == 2
            && stops[0].Attribute("id")?.Value == "colorStart"
            && stops[1].Attribute("id")?.Value == "colorEnd"
            && reverseGradient.Elements(Svg + "stop").Any() == false
            && (reverseGradient.Attribute(XLink + "href")?.Value ?? reverseGradient.Attribute("href")?.Value) == "#brandGradient";
    }

    public static XDocument? ApplyBrandColors(XDocument document, string startColor, string endColor)
    {
        if (!SupportsAutomaticRecoloring(document))
        {
            return null;
        }

        var result = new XDocument(document);
        FindById(result.Root, "colorStart")!.SetAttributeValue("stop-color", startColor);
        FindById(result.Root, "colorEnd")!.SetAttributeValue("stop-color", endColor);
        return result;
    }

    public static XDocument? CreateWhiteVariant(XDocument document)
    {
        if (!SupportsAutomaticRecoloring(document))
        {
            return null;
        }

        var result = new XDocument(document);
        foreach (var stop in result.Descendants(Svg + "stop"))
        {
            stop.SetAttributeValue("stop-color", "#FFFFFF");
        }

        foreach (var element in result.Root!.DescendantsAndSelf())
        {
            SetPaintAttribute(element, "fill", "#FFFFFF");
            SetPaintAttribute(element, "stroke", "#FFFFFF");
            SetStylePaints(element, "#FFFFFF");
        }

        return result;
    }

    public static XDocument? CreateFaviconVariant(XDocument document, string color)
    {
        if (!SupportsAutomaticRecoloring(document))
        {
            return null;
        }

        var result = new XDocument(document);
        foreach (var element in result.Root!.DescendantsAndSelf())
        {
            SetPaintAttribute(element, "fill", color, replaceReferences: true);
            SetPaintAttribute(element, "stroke", color, replaceReferences: true);
            SetPaintAttribute(element, "stop-color", color);
            SetStylePaints(element, color);
        }

        foreach (var element in result.Descendants().Where(element =>
            element.Name.LocalName is "defs" or "metadata" or "namedview").ToArray())
        {
            element.Remove();
        }

        return result;
    }

    public static string Serialize(XDocument document) => document.ToString(SaveOptions.DisableFormatting);

    private static XElement? FindById(XElement? root, string id) =>
        root?.DescendantsAndSelf().FirstOrDefault(element => element.Attribute("id")?.Value == id);

    private static void SetPaintAttribute(XElement element, string name, string color, bool replaceReferences = false)
    {
        var attribute = element.Attributes().FirstOrDefault(candidate =>
            candidate.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (attribute is not null
            && (replaceReferences || !attribute.Value.TrimStart().StartsWith("url(", StringComparison.OrdinalIgnoreCase)))
        {
            attribute.Value = color;
        }
    }

    private static void SetStylePaints(XElement element, string color)
    {
        var style = element.Attribute("style");
        if (style is null)
        {
            return;
        }

        style.Value = Regex.Replace(
            style.Value,
            @"(?i)(fill|stroke)\s*:\s*[^;]+",
            match => $"{match.Groups[1].Value}:{color}");
    }
}
