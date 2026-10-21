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

    private AuthData auth;

    // /// <summary>
    // /// Gets or sets the auth crumb.
    // /// </summary>
    // /// <value>
    // /// The crumb.
    // /// </value>
    // public string Crumb { get; private set; }
    //
    // /// <summary>
    // /// Gets or sets the auth cookie.
    // /// </summary>
    // /// <value>
    // /// The cookie.
    // /// </value>
    // public FlurlCookie Cookie { get; private set; }

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
        (Cookie, Crumb) = await GetAuth().ConfigureAwait(false);
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

        if (cookie == null)
        {
            throw new Exception("Failed to obtain Yahoo auth cookie.");
        }

        crumb = await "https://query1.finance.yahoo.com/v1/test/getcrumb"
            .WithCookie(cookie.Name, cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync()
            .ReceiveString()
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(crumb))
        {
            throw new Exception("Failed to retrieve Yahoo crumb.");
        }

        return new AuthData(cookie, crumb);
    }

    // public IFlurlRequest AddAuth(Url url)
    // {
    //     return url.SetQueryParam("crumb", Crumb)
    //         .WithCookies(Cookie)
    //         .WithHeader(UserAgentKey, UserAgentValue);
    // }

    public async Task SleepBetweenRequests(CancellationToken cancellationToken)
    {
        if (rateLimiter.Delay.TotalMilliseconds > 100)
            await Task.Delay(rateLimiter.Delay, cancellationToken).ConfigureAwait(false);
    }

    public async Task<T> DoRequest<T>(Func<CancellationToken, Task<T>> request, CancellationToken cancellationToken)
    {
        var currentAttempt = AttemptCount;

        if (Crumb == null)
            await UpdateCrumb().ConfigureAwait(false);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await rateLimiter.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (FlurlHttpException e) when (e.Call.Response?.StatusCode == (int) HttpStatusCode.Unauthorized)
            {
                if (AttemptCount < 0)
                {
                    await SleepBetweenRequests(cancellationToken).ConfigureAwait(false);
                    await UpdateCrumb().ConfigureAwait(false);
                    continue;
                }

                if (currentAttempt > 0)
                {
                    currentAttempt--;
                    await SleepBetweenRequests(cancellationToken).ConfigureAwait(false);
                    await UpdateCrumb().ConfigureAwait(false);
                    continue;
                }

                throw;
            }
        }
    }
    
    private async Task RefreshAuthCoreAsync()
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

        if (cookie == null)
        {
            throw new Exception("Failed to obtain Yahoo auth cookie.");
        }

        crumb = await "https://query1.finance.yahoo.com/v1/test/getcrumb"
            .WithCookie(cookie.Name, cookie.Value)
            .WithHeader(UserAgentKey, UserAgentValue)
            .GetAsync()
            .ReceiveString()
            .ConfigureAwait(false);

        if (string.IsNullOrEmpty(crumb))
        {
            throw new Exception("Failed to retrieve Yahoo crumb.");
        }

        auth = new AuthData(cookie, crumb);
    }

    internal async Task<T> GetJsonCoreAsync<T>(Url url, CancellationToken cancellationToken)
    {
        if (auth == null)
            await RefreshAuthCoreAsync().ConfigureAwait(false);

        for (var attempt = 0; attempt < AttemptCount; attempt++)
        {
            var currentAuth = auth;

            try
            {
                return await rateLimiter.ExecuteAsync(
                    ctx => url
                        .SetQueryParam("crumb", currentAuth.Crumb)
                        .WithCookies(currentAuth.Cookie)
                        .WithHeader(UserAgentKey, UserAgentValue)
                        .GetAsync(ctx)
                        .ReceiveJson<T>(),
                    cancellationToken).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when (IsAuthError(ex) && attempt < AttemptCount)
            {
                await RefreshAuthCoreAsync().ConfigureAwait(false);
            }
        }
    }

    private static bool IsAuthError(FlurlHttpException ex)
    {
        var status = ex.Call.Response?.StatusCode;

        return status == (int) HttpStatusCode.Unauthorized;
    }

    record AuthData(FlurlCookie Cookie, string Crumb);
}