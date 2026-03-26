using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;

namespace AutomationFramework.Tests.Reporting;


public static class ExtentReportManager
{
    private static readonly object Gate = new();

    private static ExtentReports? _extent;
    private static ExtentSparkReporter? _spark;

    public static string ReportDirectory { get; private set; } = Path.Combine(Directory.GetCurrentDirectory(), "TestResults", "Extent");

    public static void InitReport()
    {
        lock (Gate)
        {
            if (_extent is not null)
                return;

            var work = TestContext.CurrentContext?.WorkDirectory;
            if (!string.IsNullOrWhiteSpace(work))
                ReportDirectory = Path.GetFullPath(Path.Combine(work, "TestResults", "Extent"));

            Directory.CreateDirectory(ReportDirectory);

            var reportPath = Path.Combine(ReportDirectory, "index.html");
            _spark = new ExtentSparkReporter(reportPath)
            {
                Config =
                {
                    ReportName = "UI automation run",
                    DocumentTitle = "AutomationFramework test report",
                    Theme = Theme.Standard,
                },
            };

            _extent = new ExtentReports();
            _extent.AttachReporter(_spark);
        }
    }

    public static ExtentTest CreateTest(string name)
    {
        lock (Gate)
        {
            EnsureInitialized();
            return _extent!.CreateTest(name);
        }
    }

    public static void FlushReport()
    {
        lock (Gate)
        {
            _extent?.Flush();
        }
    }

    private static void EnsureInitialized()
    {
        if (_extent is null)
            throw new InvalidOperationException($"{nameof(InitReport)} must run before creating Extent tests (see AssemblyReportHooks).");
    }
}
