using System.Text.RegularExpressions;

namespace Portfolio.Application.VisualIdentity;

public static partial class VisualIdentityValidator
{
    public const string DefaultPrimaryColor = "#00A3FF";
    public const string DefaultSecondaryColor = "#3E00D1";
    public const string DefaultAccentColor = "#C98F42";
    public const long MaximumAssetSizeBytes = 10 * 1024 * 1024;

    public static IReadOnlyList<VisualIdentityValidationError> Validate(VisualIdentityEditRequest request)
    {
        var errors = new List<VisualIdentityValidationError>();
        ValidateColor(request.PrimaryColor, nameof(request.PrimaryColor), errors);
        ValidateColor(request.SecondaryColor, nameof(request.SecondaryColor), errors);
        ValidateColor(request.AccentColor, nameof(request.AccentColor), errors);

        if (request.LogoColorsLinkedToPalette
            && (request.LogoColorStartOverride is not null || request.LogoColorEndOverride is not null))
        {
            errors.Add(new("LogoColorsLinkedToPalette", "VisualIdentityLinkedColorsCannotHaveOverrides"));
        }
        else if (!request.LogoColorsLinkedToPalette)
        {
            ValidateColor(request.LogoColorStartOverride, nameof(request.LogoColorStartOverride), errors);
            ValidateColor(request.LogoColorEndOverride, nameof(request.LogoColorEndOverride), errors);
        }

        return errors;
    }

    public static bool IsHexColor(string? value) => value is not null && HexColorRegex().IsMatch(value);

    public static string NormalizeHexColor(string value) => value.ToUpperInvariant();

    public static string ContrastingTextColor(string? color)
    {
        if (!IsHexColor(color))
        {
            return "#000000";
        }

        var red = Convert.ToInt32(color![1..3], 16) / 255d;
        var green = Convert.ToInt32(color[3..5], 16) / 255d;
        var blue = Convert.ToInt32(color[5..7], 16) / 255d;
        var luminance = 0.2126 * Linearize(red) + 0.7152 * Linearize(green) + 0.0722 * Linearize(blue);
        var whiteContrast = 1.05 / (luminance + 0.05);
        var blackContrast = (luminance + 0.05) / 0.05;
        return blackContrast >= whiteContrast ? "#000000" : "#FFFFFF";
    }

    private static void ValidateColor(
        string? value,
        string field,
        ICollection<VisualIdentityValidationError> errors)
    {
        if (!IsHexColor(value))
        {
            errors.Add(new(field, "VisualIdentityColorInvalid"));
        }
    }

    private static double Linearize(double channel) => channel <= 0.04045
        ? channel / 12.92
        : Math.Pow((channel + 0.055) / 1.055, 2.4);

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexColorRegex();
}
