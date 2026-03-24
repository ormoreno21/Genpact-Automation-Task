using System.Text.RegularExpressions;

namespace AutomationFramework.Tests.Utils;

/// <summary>
/// Shared normalization so UI-extracted text and API-extracted text are comparable.
/// </summary>
public static partial class TextNormalization
{
    private static readonly Regex FootnoteMarkers = FootnoteMarkersRegex();
    private static readonly Regex NonWordChars = NonWordCharsRegex();
    private static readonly Regex Whitespace = WhitespaceRegex();

    /// <summary>
    /// Lowercases, strips footnote markers like [12], replaces punctuation with spaces, collapses whitespace.
    /// </summary>
    public static string NormalizeForWordSet(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        text = text.ToLowerInvariant();
        text = FootnoteMarkers.Replace(text, " ");
        text = NonWordChars.Replace(text, " ");
        text = Whitespace.Replace(text, " ").Trim();
        return text;
    }

    public static HashSet<string> UniqueWords(string normalizedText)
    {
        if (string.IsNullOrWhiteSpace(normalizedText))
            return new HashSet<string>(StringComparer.Ordinal);

        return normalizedText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    public static int UniqueWordCount(string normalizedText) => UniqueWords(normalizedText).Count;

    [GeneratedRegex(@"\[\d+\]", RegexOptions.Compiled)]
    private static partial Regex FootnoteMarkersRegex();

    /// <summary>Keep letters and digits (any script); map everything else to whitespace.</summary>
    [GeneratedRegex(@"[^\p{L}\p{Nd}]+", RegexOptions.Compiled)]
    private static partial Regex NonWordCharsRegex();

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();
}
