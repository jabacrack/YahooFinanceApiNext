using System;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace YahooFinanceApi;

public class RateLimiter : IDisposable
{
    public int Concurrency { get; private set; }
    public TimeSpan Delay { get; private set; }
    private readonly ConcurrencyLimiter limiter;

    public RateLimiter(): this(1, TimeSpan.FromSeconds(1))
    {
    }

    public RateLimiter(int concurrency, TimeSpan delay)
    {
        if (delay < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(delay));

        this.Concurrency = concurrency;
        this.Delay = delay;

        limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = concurrency,
            QueueLimit = 1000,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        using var lease = await limiter.AcquireAsync(1, cancellationToken);

        if (!lease.IsAcquired)
            throw new InvalidOperationException("Rate limit exceeded");

        try
        {
            return await action(cancellationToken);
        }
        finally
        {
            // ReSharper disable once MethodSupportsCancellation
            // delay should be applied in any case
            await Task.Delay(Delay);
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken = default)
    {
        using var lease = await limiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false);

        if (!lease.IsAcquired)
            throw new InvalidOperationException("Rate limit exceeded.");

        return await action(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal async Task<T> SendAsync<T>(Func<CancellationToken, Task<T>> request, CancellationToken cancellationToken)
    {
        try
        {
            return await request(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            // ReSharper disable once MethodSupportsCancellation
            // delay should be applied in any case
            await Task.Delay(Delay).ConfigureAwait(false);
        }
    }
    
    public void Dispose()
    {
        limiter.Dispose();
    }
}