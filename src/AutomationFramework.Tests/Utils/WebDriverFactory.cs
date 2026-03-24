using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AutomationFramework.Tests.Utils;

/// <summary>
/// Creates configured WebDriver instances. ChromeDriver is resolved via Selenium Manager (bundled with Selenium 4.6+).
/// </summary>
public static class WebDriverFactory
{
    public static IWebDriver CreateChrome(TestSettings settings)
    {
        var options = new ChromeOptions();
        options.AddArgument("--disable-search-engine-choice-screen");
        options.AddArgument("--no-sandbox");

        if (settings.Headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument($"--window-size={settings.WindowSize}");
        }

        var driver = new ChromeDriver(options);
        if (!settings.Headless)
            driver.Manage().Window.Maximize();

        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(settings.ImplicitWaitSeconds);
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

        return driver;
    }
}
