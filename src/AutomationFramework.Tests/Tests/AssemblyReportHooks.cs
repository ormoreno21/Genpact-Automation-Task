using AutomationFramework.Tests.Reporting;

namespace AutomationFramework.Tests;

[SetUpFixture]
public sealed class AssemblyReportHooks
{
    [OneTimeSetUp]
    public void BeforeAssembly()
    {
        ExtentReportManager.InitReport();
    }

    [OneTimeTearDown]
    public void AfterAssembly()
    {
        ExtentReportManager.FlushReport();
    }
}
