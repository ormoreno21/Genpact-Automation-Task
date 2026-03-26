using System.Net;
using System.Net.Http;
using System.Text.Json;
using AutomationFramework.Tests.Utils;

namespace AutomationFramework.Tests.Api;

public sealed class WikiApiClient : ApiClientBase
{
    public WikiApiClient()
        : base(
            new HttpClient(new SocketsHttpHandler { AutomaticDecompression = DecompressionMethods.All }),
            new Uri("https://en.wikipedia.org/"))
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "GenpactAutomationFramework/1.0 (NUnit+Selenium educational exercise; +https://en.wikipedia.org/wiki/Wikipedia:User-Agent_policy)");
    }

    
    public async Task<string> GetSectionPlainTextFromParseApiAsync(
        string pageTitle,
        string sectionAnchor,
        CancellationToken cancellationToken = default)
    {
        var index = await ResolveSectionIndexAsync(pageTitle, sectionAnchor, cancellationToken).ConfigureAwait(false);
        var wikitext = await FetchSectionWikitextAsync(pageTitle, index, cancellationToken).ConfigureAwait(false);
        return WikiMarkupCleaner.ToComparablePlainText(wikitext);
    }

    private async Task<string> ResolveSectionIndexAsync(string pageTitle, string sectionAnchor, CancellationToken cancellationToken)
    {
        var uri =
            $"w/api.php?action=parse&format=json&prop=sections&page={Uri.EscapeDataString(pageTitle)}";
        using var response = await Http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!doc.RootElement.TryGetProperty("parse", out var parse))
            throw new InvalidOperationException("Parse API response missing 'parse'.");

        if (!parse.TryGetProperty("sections", out var sections))
            throw new InvalidOperationException("Parse API response missing 'sections'.");

        foreach (var section in sections.EnumerateArray())
        {
            var anchor = section.GetProperty("anchor").GetString();
            if (string.Equals(anchor, sectionAnchor, StringComparison.Ordinal))
                return section.GetProperty("index").GetString() ?? throw new InvalidOperationException("Section index missing.");
        }

        throw new InvalidOperationException($"Section anchor '{sectionAnchor}' not found for page '{pageTitle}'.");
    }

    private async Task<string> FetchSectionWikitextAsync(string pageTitle, string sectionIndex, CancellationToken cancellationToken)
    {
        var uri =
            $"w/api.php?action=parse&format=json&prop=wikitext&page={Uri.EscapeDataString(pageTitle)}&section={Uri.EscapeDataString(sectionIndex)}";
        using var response = await Http.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (!doc.RootElement.TryGetProperty("parse", out var parse))
            throw new InvalidOperationException("Parse API response missing 'parse'.");

        if (!parse.TryGetProperty("wikitext", out var wikitext))
            throw new InvalidOperationException("Parse API response missing 'wikitext'.");

        if (!wikitext.TryGetProperty("*", out var star))
            throw new InvalidOperationException("Parse API response missing wikitext '*' body.");

        return star.GetString() ?? string.Empty;
    }
}
