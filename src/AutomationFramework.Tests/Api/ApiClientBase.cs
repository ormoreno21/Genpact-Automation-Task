namespace AutomationFramework.Tests.Api;


public abstract class ApiClientBase : IDisposable
{
    protected HttpClient Http { get; }

    protected ApiClientBase(HttpClient? httpClient = null, Uri? baseUri = null)
    {
        Http = httpClient ?? new HttpClient();
        if (baseUri is not null)
            Http.BaseAddress = baseUri;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            Http.Dispose();
    }
}
