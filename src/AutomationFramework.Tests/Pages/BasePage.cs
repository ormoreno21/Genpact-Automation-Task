using OpenQA.Selenium;

namespace AutomationFramework.Tests.Pages;

/// <summary>
/// Base page object: inject the shared <see cref="IWebDriver"/> from the active test fixture.
/// </summary>
public abstract class BasePage
{
    protected IWebDriver Driver { get; }

    protected BasePage(IWebDriver driver)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
    }
}
