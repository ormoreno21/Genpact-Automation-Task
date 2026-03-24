using System.Text.RegularExpressions;

namespace AutomationFramework.Tests.Utils;

/// <summary>
/// Converts a section slice of wikitext into plain text suitable for the same normalization pipeline as DOM innerText.
/// </summary>
public static partial class WikiMarkupCleaner
{
    private static readonly Regex HeaderLine = HeaderLineRegex();
    private static readonly Regex EditBracket = EditBracketRegex();
    private static readonly Regex ListLead = ListLeadRegex();
    private static readonly Regex RefTags = RefTagsRegex();

    public static string ToComparablePlainText(string wikitext)
    {
        if (string.IsNullOrWhiteSpace(wikitext))
            return string.Empty;

        var text = wikitext.Trim();
        text = HeaderLine.Replace(text, " ");
        text = RefTags.Replace(text, " ");
        text = StripTemplates(text);
        text = ListLead.Replace(text, " ");
        text = EditBracket.Replace(text, " ");
        return text;
    }

    private static string StripTemplates(string text)
    {
        const int maxPasses = 128;
        for (var i = 0; i < maxPasses && text.Contains("{{", StringComparison.Ordinal); i++)
        {
            var next = TemplateOnceRegex().Replace(text, " ");
            if (next.Length == text.Length && next == text)
                break;
            text = next;
        }

        return text;
    }

    [GeneratedRegex(@"(?m)^=+\s*[^=\r\n]+?\s*=+\s*", RegexOptions.Compiled)]
    private static partial Regex HeaderLineRegex();

    [GeneratedRegex(@"\[\s*edit\s*\]", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EditBracketRegex();

    [GeneratedRegex(@"(?m)^\*+\s*", RegexOptions.Compiled)]
    private static partial Regex ListLeadRegex();

    [GeneratedRegex(@"<ref\b[^>]*/>|<ref\b[^>]*>[\s\S]*?</ref>", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex RefTagsRegex();

    /// <summary>Non-greedy template peel; safe for typical citation templates (no nested {{ }}).</summary>
    [GeneratedRegex(@"\{\{[^{}]*\}\}", RegexOptions.Compiled)]
    private static partial Regex TemplateOnceRegex();
}
