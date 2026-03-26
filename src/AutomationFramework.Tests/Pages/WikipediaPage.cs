using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AutomationFramework.Tests.Pages;


public sealed class WikipediaPage : BasePage
{
    public const string PlaywrightArticleUrl = "https://en.wikipedia.org/wiki/Playwright_(software)";
    public const string DebuggingFeaturesHeadingId = "Debugging_features";
    public const string NextSectionHeadingId = "Reporters";
    public const string MicrosoftDevelopmentToolsHeadingId = "Microsoft_development_tools";

   
    public const string SkinThemePortletId = "skin-client-prefs-skin-theme";

    
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
    
        var htmlElement = Driver.FindElement(By.TagName("html"));
        var htmlClass = htmlElement.GetAttribute("class") ?? string.Empty;

     
        var bodyElement = Driver.FindElement(By.TagName("body"));
        var bodyBg = bodyElement.GetCssValue("background-color") ?? string.Empty;

     
        var isNight = htmlClass.Contains("skin-theme-clientpref-night", StringComparison.Ordinal);

     
        return new VectorThemeState(htmlClass, bodyBg, isNight);
    }

    public sealed record VectorThemeState(string HtmlClass, string BodyBackgroundCss, bool IsNightTheme);
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

    
    public LinkValidationResult ValidateTechnologyLinksUnderMicrosoftDevelopmentTools()
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
        var navboxTable = wait.Until(d => FindMicrosoftDevelopmentToolsNavboxTable(d));
        if (navboxTable is null)
            throw new NoSuchElementException("Could not find 'Microsoft development tools' navbox table.");

        ExpandMicrosoftNavboxIfCollapsed(navboxTable);

        var technologyNames = new List<string>();
        var nonLinkNames = new List<string>();

        var itemCells = navboxTable.FindElements(By.XPath(".//td[contains(@class,'navbox-list')]"));
        foreach (var cell in itemCells)
        {
            var items = cell.FindElements(By.CssSelector("li"));
            foreach (var item in items)
            {
                var itemText = (item.Text ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(itemText))
                    continue;

                technologyNames.Add(itemText);

                var anchors = item.FindElements(By.CssSelector("a[href]"));
                var hasValidLink = anchors.Any(a =>
                {
                    var href = a.GetAttribute("href");
                    return !string.IsNullOrWhiteSpace(href);
                });

                if (!hasValidLink)
                    nonLinkNames.Add(itemText);
            }

           
            var inlineAnchors = cell.FindElements(By.CssSelector("a[href]"));
            foreach (var anchor in inlineAnchors)
            {
                var text = (anchor.Text ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    technologyNames.Add(text);
            }


            var hasPlaywrightText = cell.Text.Contains("Playwright", StringComparison.OrdinalIgnoreCase);
            var hasPlaywrightLink = inlineAnchors.Any(a =>
                string.Equals((a.Text ?? string.Empty).Trim(), "Playwright", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(a.GetAttribute("href")));
            if (hasPlaywrightText && !hasPlaywrightLink)
            {
                technologyNames.Add("Playwright");
                nonLinkNames.Add("Playwright");
            }
        }

        if (technologyNames.Count == 0)
        {
            var anchors = navboxTable.FindElements(By.CssSelector("a[href]"));
            foreach (var anchor in anchors)
            {
                var text = (anchor.Text ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    technologyNames.Add(text);
            }

            var tableText = navboxTable.Text ?? string.Empty;
            if (tableText.Contains("Playwright", StringComparison.OrdinalIgnoreCase) &&
                !anchors.Any(a => string.Equals((a.Text ?? string.Empty).Trim(), "Playwright", StringComparison.OrdinalIgnoreCase)))
            {
                technologyNames.Add("Playwright");
                nonLinkNames.Add("Playwright");
            }
        }

        return new LinkValidationResult(
            technologyNames.Distinct(StringComparer.Ordinal).ToList(),
            nonLinkNames.Distinct(StringComparer.Ordinal).ToList());
    }

    private static IWebElement? FindMicrosoftDevelopmentToolsNavboxTable(ISearchContext root)
    {
        var heading = root
            .FindElements(By.XPath("//div[starts-with(@id,'Microsoft_development_tools') and contains(translate(normalize-space(.), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'microsoft development tools')]"))
            .FirstOrDefault();
        if (heading is null)
            return null;

        var navboxContainer = heading.FindElements(By.XPath("./ancestor::div[contains(@class,'navbox')][1]")).FirstOrDefault();
        if (navboxContainer is null)
            return null;

        var candidateTables = navboxContainer.FindElements(By.XPath(".//table[contains(@class,'navbox-inner')]"));
        foreach (var table in candidateTables)
        {
            var cells = table.FindElements(By.XPath(".//td[contains(@class,'navbox-list')]"));
            if (cells.Count > 0)
                return table;
        }

        return candidateTables.FirstOrDefault();
    }

    private void ExpandMicrosoftNavboxIfCollapsed(IWebElement navboxTable)
    {
        var showButton = navboxTable
            .FindElements(By.XPath("//*[@id='mw-content-text']/div[2]/div[17]/table/tbody/tr[1]/th/button"))    
            .FirstOrDefault(a => string.Equals((a.Text ?? string.Empty).Trim(), "show", StringComparison.OrdinalIgnoreCase));

        if (showButton is null)
            return;

        showButton.Click();

        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        wait.Until(_ =>
            navboxTable.FindElements(By.XPath("//*[@id='mw-content-text']/div[2]/div[17]/table/tbody/tr[1]/th/button"))
                .Any(a => string.Equals((a.Text ?? string.Empty).Trim(), "hide", StringComparison.OrdinalIgnoreCase)));
    }
}
