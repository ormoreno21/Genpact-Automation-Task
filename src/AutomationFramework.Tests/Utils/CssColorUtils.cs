using System.Globalization;
using System.Text.RegularExpressions;

namespace AutomationFramework.Tests.Utils;

public static partial class CssColorUtils
{
    /// <summary>Returns approximate relative luminance (0–1) for rgb/rgba strings from <see cref="IWebDriver"/> computed styles.</summary>
    public static bool TryGetRgbLuminance(string? cssColor, out double luminance)
    {
        luminance = 0;
        if (string.IsNullOrWhiteSpace(cssColor))
            return false;

        var m = RgbRegex().Match(cssColor.Trim());
        if (!m.Success)
            return false;

        var r = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) / 255.0;
        var g = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture) / 255.0;
        var b = int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture) / 255.0;

        luminance = 0.2126 * r + 0.7152 * g + 0.0722 * b;
        return true;
    }

    [GeneratedRegex(@"rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)", RegexOptions.Compiled)]
    private static partial Regex RgbRegex();
}
