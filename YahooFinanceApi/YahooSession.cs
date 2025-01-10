using Flurl.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Net;
using Flurl;

namespace YahooFinanceApi;

public delegate Task<T> RequestDelegate<T>(Func<Url, Url> addAuth);
/// <summary>
/// Holds state for Yahoo HTTP calls
/// </summary>
internal class YahooSession
{
    private  SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    private  static Dictionary<string, TimeZoneInfo> timeZoneCache = new();
        
    /// <summary>
    /// The user agent key for HTTP Header
    /// </summary>
    public const string UserAgentKey = "User-Agent";

    /// <summary>
    /// The user agent value for HTTP Header
    /// </summary>
    public const string UserAgentValue = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.0";

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
    public  FlurlCookie Cookie { get; private set; }

    public int AttemptCount { get; } = -1;

    /// <summary>
    /// Initializes the crumb value asynchronously.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <exception cref="System.Exception">Failure to create client.</exception>
    public async Task InitCrumb(CancellationToken token = default)
    {
        if (Crumb != null)
        {
            return;
        }

        await semaphore.WaitAsync(token).ConfigureAwait(false);
        try
        {
            var response = await "https://fc.yahoo.com"
                .AllowHttpStatus("404")
                .AllowHttpStatus("500")
                .AllowHttpStatus("502")
                .WithHeader(UserAgentKey, UserAgentValue)
                .GetAsync(token)
                .ConfigureAwait(false);

            Cookie = response.Cookies.FirstOrDefault(c => c.Name == "A3");

            if (Cookie == null)
            {
                throw new Exception("Failed to obtain Yahoo auth cookie.");
            }
            else
            {
                Crumb = await "https://query1.finance.yahoo.com/v1/test/getcrumb"
                    .WithCookie(Cookie.Name, Cookie.Value)
                    .WithHeader(UserAgentKey, UserAgentValue)
                    .GetAsync(token)
                    .ReceiveString()
                    .ConfigureAwait(false);

                if (string.IsNullOrEmpty(Crumb))
                {
                    throw new Exception("Failed to retrieve Yahoo crumb.");
                }
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<T> DoRequest<T>(RequestDelegate<T> request)
    {
        try
        {
            await request(url => url.SetQueryParam("crumb", Crumb));
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == (int) HttpStatusCode.Unauthorized)
        {
            
        }
    }
}