namespace AutomationFramework.Tests.Utils;

/// <summary>
/// Central place for runtime test configuration (URLs, timeouts, headless mode).
/// Extend with JSON/env sources as the suite grows.
/// </summary>
public sealed class TestSettings
{
    public string BaseUrl { get; init; } = "https://example.com";

    public bool Headless { get; init; }

    public int ImplicitWaitSeconds { get; init; } = 10;

    public string WindowSize { get; init; } = "1920,1080";

    public static TestSettings FromEnvironment()
    {
        var headless = bool.TryParse(Environment.GetEnvironmentVariable("HEADLESS"), out var h) && h;
        var wait = int.TryParse(Environment.GetEnvironmentVariable("IMPLICIT_WAIT_SECONDS"), out var w)
            ? w
            : 10;

        return new TestSettings
        {
            BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "https://example.com",
            Headless = headless,
            ImplicitWaitSeconds = wait,
            WindowSize = Environment.GetEnvironmentVariable("WINDOW_SIZE") ?? "1920,1080",
        };
    }
}
