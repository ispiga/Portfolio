using Portfolio.Application.VisualIdentity;
using Xunit;

namespace Portfolio.Tests;

public sealed class VisualIdentityValidatorTests
{
    [Theory]
    [InlineData("#00A3FF", "#000000")]
    [InlineData("#3E00D1", "#FFFFFF")]
    [InlineData("#C98F42", "#000000")]
    [InlineData("not-a-color", "#000000")]
    public void Contrasting_text_color_returns_high_contrast_color(string? color, string expected)
    {
        Assert.Equal(expected, VisualIdentityValidator.ContrastingTextColor(color));
    }

    [Fact]
    public void Color_validation_accepts_hex_values_and_rejects_invalid_values()
    {
        var request = new VisualIdentityEditRequest(
            "#00a3ff",
            "#3E00D1",
            "#C98F42",
            true,
            null,
            null);

        Assert.Empty(VisualIdentityValidator.Validate(request));
        Assert.False(VisualIdentityValidator.IsHexColor("blue"));
        Assert.Equal("#00A3FF", VisualIdentityValidator.NormalizeHexColor("#00a3ff"));
    }
}
