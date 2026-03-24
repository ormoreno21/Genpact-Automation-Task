using AutomationFramework.Tests.Api;
using AutomationFramework.Tests.Pages;
using AutomationFramework.Tests.Utils;

namespace AutomationFramework.Tests;

[TestFixture]
public sealed class WikipediaDebuggingFeaturesTests : BaseTest
{
    private const string PageTitle = "Playwright_(software)";
    private const string SectionAnchor = WikipediaPage.DebuggingFeaturesHeadingId;

    [Test]
    public async Task PlaywrightArticle_DebuggingFeatures_UniqueWordCounts_Match_Ui_and_ParseApi()
    {
        var wikiPage = new WikipediaPage(Driver);
        wikiPage.OpenPlaywrightArticle();

        var uiRaw = wikiPage.GetDebuggingFeaturesSectionText();
        Assert.That(uiRaw, Is.Not.Empty, "UI section body should not be empty.");

        using var api = new WikiApiClient();
        var apiRaw = await api.GetSectionPlainTextFromParseApiAsync(PageTitle, SectionAnchor).ConfigureAwait(false);
        Assert.That(apiRaw, Is.Not.Empty, "Parse API (wikitext) section body should not be empty.");

        var normalizedUi = TextNormalization.NormalizeForWordSet(uiRaw);
        var normalizedApi = TextNormalization.NormalizeForWordSet(apiRaw);

        var uiUnique = TextNormalization.UniqueWords(normalizedUi);
        var apiUnique = TextNormalization.UniqueWords(normalizedApi);

        Assert.That(
            apiUnique.Count,
            Is.EqualTo(uiUnique.Count),
            () =>
                $"""
                Unique word counts differ.
                UI unique count: {uiUnique.Count}
                API unique count: {apiUnique.Count}
                Only in UI: {string.Join(", ", uiUnique.Except(apiUnique).OrderBy(w => w))}
                Only in API: {string.Join(", ", apiUnique.Except(uiUnique).OrderBy(w => w))}
                """);
    }

    [Test]
    public void PlaywrightArticle_MicrosoftDevelopmentTools_AllTechnologies_AreTextLinks()
    {
        var wikiPage = new WikipediaPage(Driver);
        wikiPage.OpenPlaywrightArticle();

        var result = wikiPage.ValidateTechnologyLinksUnderMicrosoftDevelopmentTools();
        Assert.That(
            result.NonLinkTechnologyNames,
            Does.Not.Contain("__SECTION_NOT_FOUND__"),
            "Could not find 'Microsoft development tools' section on the page.");

        Assert.That(result.TechnologyNames, Is.Not.Empty, "No technologies were found under 'Microsoft development tools'.");
        Assert.That(
            result.NonLinkTechnologyNames,
            Is.Empty,
            () => $"The following technologies are not valid text links (<a href=...>): {string.Join(", ", result.NonLinkTechnologyNames)}");
    }
}
