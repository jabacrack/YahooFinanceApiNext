using System;
using System.Threading;
using System.Threading.Tasks;
using Flurl;

namespace YahooFinanceApi;

internal sealed class YahooRequestContext
{
    private readonly YahooSession session;
    private readonly RateLimiter rateLimiter;

    internal YahooRequestContext(YahooSession session, RateLimiter rateLimiter)
    {
        this.session = session;
        this.rateLimiter = rateLimiter;
    }

    internal Task<T> SendRawAsync<T>(Func<CancellationToken, Task<T>> request, CancellationToken cancellationToken)
    {
        return rateLimiter.SendAsync(request, cancellationToken);
    }

    internal Task<T> GetJsonAsync<T>(Url url, CancellationToken cancellationToken)
    {
        return session.GetJsonCoreAsync<T>(this, url, cancellationToken);
    }
}