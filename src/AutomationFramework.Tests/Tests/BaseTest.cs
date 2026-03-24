using AventStack.ExtentReports;
using AventStack.ExtentReports.MarkupUtils;
using AutomationFramework.Tests.Reporting;
using AutomationFramework.Tests.Utils;
using NUnit.Framework.Interfaces;
using OpenQA.Selenium;

namespace AutomationFramework.Tests;

/// <summary>
/// Base fixture for UI tests: spins up Chrome before each test and quits cleanly after.
/// Inherit from this class for concrete test fixtures (do not add test cases here).
/// </summary>
[TestFixture]
public abstract class BaseTest
{
    protected IWebDriver Driver { get; private set; } = null!;

    protected TestSettings Settings { get; private set; } = null!;

    /// <summary>Current Extent test node (Bonus reporting).</summary>
    protected ExtentTest? Extent { get; private set; }

    [SetUp]
    public virtual void SetUp()
    {
        Settings = TestSettings.FromEnvironment();
        Driver = WebDriverFactory.CreateChrome(Settings);
        Extent = ExtentReportManager.CreateTest(TestContext.CurrentContext.Test.FullName);
    }

    [TearDown]
    public virtual void TearDown()
    {
        try
        {
            LogExtentOutcome();
        }
        finally
        {
            try
            {
                Driver?.Quit();
            }
            finally
            {
                Driver?.Dispose();
                Driver = null!;
                Extent = null;
            }
        }
    }

    private void LogExtentOutcome()
    {
        if (Extent is null)
            return;

        switch (TestContext.CurrentContext.Result.Outcome.Status)
        {
            case TestStatus.Passed:
                Extent.Pass("Passed");
                break;
            case TestStatus.Failed:
            {
                var message = TestContext.CurrentContext.Result.Message ?? "Failed";
                Extent.Fail(message);
                var trace = TestContext.CurrentContext.Result.StackTrace;
                if (!string.IsNullOrWhiteSpace(trace))
                    Extent.Info(MarkupHelper.CreateCodeBlock(trace));
                break;
            }
            case TestStatus.Skipped:
                Extent.Skip(TestContext.CurrentContext.Result.Message ?? "Skipped");
                break;
            default:
                Extent.Warning($"Outcome: {TestContext.CurrentContext.Result.Outcome.Status}");
                break;
        }
    }
}
