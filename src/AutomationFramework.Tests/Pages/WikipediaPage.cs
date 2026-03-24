using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.Tests.Pages;

/// <summary>
/// Wikipedia Vector 2022 interactions for the Playwright article and chrome (Appearance / theme).
/// </summary>
public sealed class WikipediaPage : BasePage
{
    public const string PlaywrightArticleUrl = "https://en.wikipedia.org/wiki/Playwright_(software)";
    public const string DebuggingFeaturesHeadingId = "Debugging_features";
    public const string NextSectionHeadingId = "Reporters";
    public const string MicrosoftDevelopmentToolsHeadingId = "Microsoft_development_tools";

    /// <summary>MediaWiki client preference portlet for Vector "Color (beta)" (night/day/os).</summary>
    public const string SkinThemePortletId = "skin-client-prefs-skin-theme";

    /// <summary>Radio input id for dark / night theme (<c>vector-theme=night</c>).</summary>
    public const string SkinThemeNightInputId = "skin-client-pref-skin-theme-value-night";

    public WikipediaPage(IWebDriver driver)
        : base(driver)
    {
    }

    public void OpenPlaywrightArticle(TimeSpan? timeout = null, bool waitForDebuggingHeadings = true)
    {
        Driver.Navigate().GoToUrl(PlaywrightArticleUrl);

        var wait = new WebDriverWait(Driver, timeout ?? TimeSpan.FromSeconds(25));
        wait.Until(d => d.FindElement(By.TagName("body")));
        if (!waitForDebuggingHeadings)
            return;

        wait.Until(d => d.FindElement(By.Id(DebuggingFeaturesHeadingId)));
        wait.Until(d => d.FindElement(By.Id(NextSectionHeadingId)));
    }

    /// <summary>Waits for Vector JS and the Appearance menu's Color (beta) control bundle.</summary>
    public void WaitForVectorAppearanceColorControls(TimeSpan? timeout = null)
    {
        var total = timeout ?? TimeSpan.FromSeconds(50);
        var wait = new WebDriverWait(Driver, total);
        wait.Until(d =>
        {
            var cls = d.FindElement(By.TagName("html")).GetAttribute("class") ?? string.Empty;
            return cls.Contains("client-js", StringComparison.Ordinal);
        });

        var rootClass = Driver.FindElement(By.TagName("html")).GetAttribute("class") ?? string.Empty;
        if (rootClass.Contains("vector-feature-night-mode-disabled", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Vector night mode / Color (beta) is disabled on this page response (vector-feature-night-mode-disabled).");
        }

        try
        {
            Driver.Manage().Window.Size = new Size(1920, 1080);
        }
        catch (WebDriverException)
        {
            // Headless or driver may not support programmatic resize; Chrome --window-size still applies.
        }

        wait.Until(d => d.FindElements(By.Id("vector-appearance")).Count > 0);

        var deadline = DateTime.UtcNow + total;
        while (DateTime.UtcNow < deadline)
        {
            if (IsNightThemeControlReady(Driver))
            {
                new WebDriverWait(Driver, TimeSpan.FromSeconds(5)).Until(d => IsNightThemeControlReady(d));
                return;
            }

            try
            {
                var panel = Driver.FindElement(By.Id("vector-appearance"));
                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", panel);
            }
            catch (NoSuchElementException)
            {
                // Appears once Vector mounts the sidebar chrome.
            }

            ToggleHeaderAppearanceDropdownIfClosed();
            Thread.Sleep(450);
        }

        throw new WebDriverTimeoutException(
            $"Timed out waiting for #{SkinThemeNightInputId}. Open Appearance (header or sidebar) and ensure Color (beta) is available.");
    }

    private static bool IsNightThemeControlReady(IWebDriver driver)
    {
        var found = driver.FindElements(By.Id(SkinThemeNightInputId));
        if (found.Count == 0)
            return false;

        var el = found[0];
        try
        {
            if (!el.Enabled)
                return false;

            var js = (IJavaScriptExecutor)driver;
            var visible = js.ExecuteScript(
                """
                var e = arguments[0];
                if (!e) return false;
                var r = e.getBoundingClientRect();
                var st = getComputedStyle(e);
                return r.width > 0 && r.height > 0 && st.visibility !== 'hidden' && st.display !== 'none';
                """,
                el);
            return visible is true;
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    private void ToggleHeaderAppearanceDropdownIfClosed()
    {
        var checkboxes = Driver.FindElements(By.Id("vector-appearance-dropdown-checkbox"));
        if (checkboxes.Count == 0)
            return;

        var cb = checkboxes[0];
        try
        {
            if (cb.Selected)
                return;
        }
        catch (StaleElementReferenceException)
        {
            return;
        }

        var labels = Driver.FindElements(By.CssSelector("label[for='vector-appearance-dropdown-checkbox']"));
        var js = (IJavaScriptExecutor)Driver;
        if (labels.Count > 0)
            js.ExecuteScript("arguments[0].click();", labels[0]);
        else
            js.ExecuteScript("arguments[0].click();", cb);
    }

    public void ScrollVectorAppearancePanelIntoView()
    {
        var panel = Driver.FindElement(By.Id("vector-appearance"));
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", panel);
    }

    /// <summary>Selects dark mode via the right Appearance sidebar (Color beta → Night).</summary>
    public void SelectDarkColorThemeFromAppearanceSidebar()
    {
        WaitForVectorAppearanceColorControls();
        ScrollVectorAppearancePanelIntoView();

        var nightInput = Driver.FindElement(By.Id(SkinThemeNightInputId));
        if (nightInput.Selected)
            return;

        var js = (IJavaScriptExecutor)Driver;
        js.ExecuteScript("arguments[0].click();", nightInput);

        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
        wait.Until(d =>
        {
            var cls = d.FindElement(By.TagName("html")).GetAttribute("class") ?? string.Empty;
            return cls.Contains("skin-theme-clientpref-night", StringComparison.Ordinal);
        });
    }

    public VectorThemeState ReadVectorThemeState()
    {
        const string script = """
            var h = document.documentElement;
            var cls = h.className || '';
            var bg = window.getComputedStyle(document.body).backgroundColor || '';
            return cls + '|||' + bg;
            """;

        var js = (IJavaScriptExecutor)Driver;
        var raw = js.ExecuteScript(script)?.ToString() ?? "|||";
        var parts = raw.Split("|||", 2, StringSplitOptions.None);
        var htmlClass = parts.Length > 0 ? parts[0] : string.Empty;
        var bodyBg = parts.Length > 1 ? parts[1] : string.Empty;
        var isNight = htmlClass.Contains("skin-theme-clientpref-night", StringComparison.Ordinal);
        return new VectorThemeState(htmlClass, bodyBg, isNight);
    }

    public sealed record VectorThemeState(string HtmlClass, string BodyBackgroundCss, bool IsNightTheme);

    /// <summary>
    /// Walks siblings between the Vector heading wrappers to mirror the article subsection body (paragraph + list, excluding the next heading).
    /// </summary>
    public string GetSectionTextBetweenHeadings(string startHeadingId, string nextHeadingId)
    {
        const string script = """
            var startH = document.getElementById(arguments[0]);
            var endH = document.getElementById(arguments[1]);
            if (!startH || !endH) { return ''; }
            var startContainer = startH.closest('div.mw-heading') || startH;
            var endContainer = endH.closest('div.mw-heading') || endH;
            var el = startContainer.nextElementSibling;
            var parts = [];
            while (el && el !== endContainer) {
                parts.push(el.innerText || '');
                el = el.nextElementSibling;
            }
            return parts.join('\n');
            """;

        var js = (IJavaScriptExecutor)Driver;
        var result = js.ExecuteScript(script, startHeadingId, nextHeadingId);
        return result?.ToString() ?? string.Empty;
    }

    public string GetDebuggingFeaturesSectionText() =>
        GetSectionTextBetweenHeadings(DebuggingFeaturesHeadingId, NextSectionHeadingId);

    public sealed record LinkValidationResult(IReadOnlyList<string> TechnologyNames, IReadOnlyList<string> NonLinkTechnologyNames);

    /// <summary>
    /// Validates that every listed technology under the "Microsoft development tools" subsection is rendered as a text link (<a href=...>).
    /// </summary>
    public LinkValidationResult ValidateTechnologyLinksUnderMicrosoftDevelopmentTools()
    {
        const string script = """
            function findHeadingElement() {
              var byId = document.getElementById(arguments[0]);
              if (byId) return byId;

              var normalizedTarget = 'microsoft development tools';
              var headings = document.querySelectorAll('h2, h3, h4, h5, h6');
              for (var i = 0; i < headings.length; i++) {
                var txt = (headings[i].innerText || '').trim().toLowerCase();
                if (txt === normalizedTarget) return headings[i];
              }
              return null;
            }

            var startHeading = findHeadingElement();
            if (!startHeading) {
              return { technologyNames: [], nonLinkTechnologyNames: ['__SECTION_NOT_FOUND__'] };
            }

            var startContainer = startHeading.closest('div.mw-heading') || startHeading;
            var cursor = startContainer.nextElementSibling;
            var technologyNames = [];
            var nonLinkTechnologyNames = [];

            while (cursor) {
              if (cursor.matches && cursor.matches('div.mw-heading, h2, h3, h4, h5, h6')) break;
              if (cursor.tagName && cursor.tagName.toLowerCase() === 'ul') {
                var items = cursor.querySelectorAll(':scope > li');
                for (var i = 0; i < items.length; i++) {
                  var li = items[i];
                  var text = (li.innerText || '').trim();
                  if (!text) continue;
                  technologyNames.push(text);

                  var anchor = li.querySelector('a');
                  var href = anchor ? (anchor.getAttribute('href') || '').trim() : '';
                  if (!anchor || !href) nonLinkTechnologyNames.push(text);
                }
              }
              cursor = cursor.nextElementSibling;
            }

            return { technologyNames: technologyNames, nonLinkTechnologyNames: nonLinkTechnologyNames };
            """;

        var js = (IJavaScriptExecutor)Driver;
        var raw = js.ExecuteScript(script, MicrosoftDevelopmentToolsHeadingId);
        if (raw is not Dictionary<string, object?> map)
            throw new InvalidOperationException("Unexpected JS return payload for technology link validation.");

        static IReadOnlyList<string> ToStringList(object? value)
        {
            if (value is not IEnumerable<object?> items)
                return Array.Empty<string>();

            return items
                .Select(x => x?.ToString())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToList();
        }

        return new LinkValidationResult(
            ToStringList(map.TryGetValue("technologyNames", out var t) ? t : null),
            ToStringList(map.TryGetValue("nonLinkTechnologyNames", out var n) ? n : null));
    }
}
