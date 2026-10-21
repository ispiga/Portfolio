using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

namespace Portfolio.Web.Authentication;

public static class ImportMapCspIntegrity
{
    public static string? GetIntegrity(HttpContext? context)
    {
        var definition = context?.GetEndpoint()?.Metadata.GetMetadata<ImportMapDefinition>();
        return definition is null ? null : GetIntegrity(definition);
    }

    public static string GetIntegrity(ImportMapDefinition definition)
    {
        var json = definition.ToString().ReplaceLineEndings("\n");
        var bytes = new UTF8Encoding().GetBytes(json);
        return $"sha256-{Convert.ToBase64String(SHA256.HashData(bytes))}";
    }

    public static string GetReportOnlyPolicy(string? integrity) =>
        integrity is null
            ? PortfolioCspReportOnly.Policy
            : PortfolioCspReportOnly.Policy.Replace(
                "script-src 'self';",
                $"script-src 'self' '{integrity}';",
                StringComparison.Ordinal);
}