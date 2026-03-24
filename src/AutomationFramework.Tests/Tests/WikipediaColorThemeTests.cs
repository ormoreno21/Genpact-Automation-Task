using AutomationFramework.Tests.Pages;
using AutomationFramework.Tests.Utils;

namespace AutomationFramework.Tests;

[TestFixture]
public sealed class WikipediaColorThemeTests : BaseTest
{
    [Test]
    public void PlaywrightArticle_Appearance_ColorBeta_Dark_UpdatesDomAndBackground()
    {
        var page = new WikipediaPage(Driver);
        page.OpenPlaywrightArticle(waitForDebuggingHeadings: false);

        var before = page.ReadVectorThemeState();
        Assert.That(before.HtmlClass, Does.Contain("client-js"), "Vector should run with client JS enabled.");

        page.SelectDarkColorThemeFromAppearanceSidebar();

        var after = page.ReadVectorThemeState();
        Assert.That(
            after.IsNightTheme,
            Is.True,
            $"Expected night theme class on <html>. Actual classes: {after.HtmlClass}");

        Assert.That(
            CssColorUtils.TryGetRgbLuminance(after.BodyBackgroundCss, out var luminance),
            Is.True,
            $"Could not parse body background from computed style: '{after.BodyBackgroundCss}'.");

        Assert.That(
            luminance,
            Is.LessThan(0.5),
            $"Body background should read as a dark surface after theme change. CSS: '{after.BodyBackgroundCss}' (luminance≈{luminance:F3}).");

        Extent?.Info($"Night theme on. Body background computed: {after.BodyBackgroundCss}");
    }
}
