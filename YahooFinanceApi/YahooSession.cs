using Flurl.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Net;
using Flurl;

namespace YahooFinanceApi;

public delegate Task<T> RequestDelegate<T>(Func<Url, IFlurlRequest> addAuth, CancellationToken cancellationToken);

/// <summary>
/// Holds state for Yahoo HTTP calls
/// </summary>
public class YahooSession
{
    private readonly RateLimiter rateLimiter;
    private readonly object crumbLock = new();
    private Task crumbUpdateTask;
    private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Holds state for Yahoo HTTP calls
    /// </summary>
    public YahooSession(RateLimiter rateLimiter = null)
    {
        this.rateLimiter = rateLimiter ?? new RateLimiter();
    }

    private static Dictionary<string, TimeZoneInfo> timeZoneCache = new();

    /// <summary>
    /// The user agent key for HTTP Header
    /// </summary>
    public const string UserAgentKey = "User-Agent";

    /// <summary>
    /// The user agent value for HTTP Header
    /// </summary>
    public const string UserAgentValue =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.0";

    /// <summary>
    /// Gets or sets the auth crumb.
    /// </summary>
    /// <value>
    /// The crumb.
    /// </value>
    public string Crumb { get; private set; }

    /// <summary>
    /// Gets or sets the auth cookie.
    /// </summary>
    /// <value>
    /// The cookie.
    /// </value>
    public FlurlCookie Cookie { get; private set; }

    public int AttemptCount { get; } = 5;

    /// <summary>
    /// Initializes the crumb value asynchronously.
    /// </summary>
    public Task UpdateCrumb()
    {
        lock (crumbLock)
        {
            if (crumbUpdateTask is {IsCompleted: false})
                return crumbUpdateTask;

            crumbUpdateTask = ExecuteGetCrumb();
            return crumbUpdateTask;
        }
    }

    private async Task ExecuteGetCrumb()
    {
        await Task.Yield();
        (Cookie, Crumb) = await GetAuth();
    }

    private async Task<AuthData> GetAuth()
    {
        FlurlCookie cookie;
        string crumb;

        var response = await "https://fc.yahoo.com"
            .AllowHttpStatus("404")
            .AllowHttpStatus("500")
            .AllowHttpStatus("502")
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync()
            .ConfigureAwait(false);

        cookie = response.Cookies.FirstOrDefault(c => c.Name == "A3");

        if (Cookie == null)
        {
            throw new Exception("Failed to obtain Yahoo auth cookie.");
        }
        else
        {
            crumb = await "https://query1.finance.yahoo.com/v1/test/getcrumb"
                .WithCookie(Cookie.Name, Cookie.Value)
                .WithHeader(UserAgentKey, UserAgentValue)
                .GetAsync()
                .ReceiveString()
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(Crumb))
            {
                throw new Exception("Failed to retrieve Yahoo crumb.");
            }
        }

        return new AuthData(cookie, crumb);
    }

    public IFlurlRequest AddAuth(Url url)
    {
        return url.SetQueryParam("crumb", Crumb)
            .WithCookies(Cookie)
            .WithHeader(UserAgentKey, UserAgentValue);
    }
    
    public async Task<T> DoRequest<T>(RequestDelegate<T> request, CancellationToken cancellationToken)
    {
        var currentAttempt = AttemptCount;

        if (Crumb == null)
            await UpdateCrumb();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await rateLimiter.ExecuteAsync(request(
                    url => url.SetQueryParam("crumb", Crumb).WithCookie(Cookie.Name, Cookie.Value).WithHeader(UserAgentKey, UserAgentValue),
                    cancellationToken), cancellationToken);
                await ;
            }
            catch (FlurlHttpException e) when (e.Call.Response?.StatusCode == (int) HttpStatusCode.Unauthorized)
            {
                if (AttemptCount < 0)
                {
                    await Task.Delay(rateLimiter.Delay, cancellationToken);
                    await UpdateCrumb();
                    continue;
                }

                if (currentAttempt > 0)
                {
                    currentAttempt--;
                    await Task.Delay(rateLimiter.Delay, cancellationToken);
                    await UpdateCrumb();
                    continue;
                }

                throw;
            }
        }
    }

    record AuthData(FlurlCookie Cookie, string Crumb);
}