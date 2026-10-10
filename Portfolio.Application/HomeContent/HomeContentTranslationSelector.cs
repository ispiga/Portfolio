using System.Globalization;

namespace Portfolio.Application.HomeContent;

public static class HomeContentTranslationSelector
{
    public static string? Select(string? requested, string? spanish) =>
        !string.IsNullOrWhiteSpace(requested) ? requested : spanish;

    public static string CurrentLanguageCode =>
        CultureInfo.CurrentUICulture.Name.Equals("en-US", StringComparison.OrdinalIgnoreCase) ? "en-US" : "es-ES";
}