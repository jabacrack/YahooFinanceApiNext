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

    public RateLimiter()
    {
        Concurrency = 1;
        Delay = TimeSpan.FromSeconds(1);

        limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = Concurrency,
            QueueLimit = 1000,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    public RateLimiter(int concurrency, TimeSpan delay)
    {
        this.Concurrency = concurrency;
        this.Delay = delay;

        limiter = new ConcurrencyLimiter(new ConcurrencyLimiterOptions
        {
            PermitLimit = concurrency,
            QueueLimit = 1000,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        using var lease = await limiter.AcquireAsync(1, cancellationToken);

        if (!lease.IsAcquired)
            throw new InvalidOperationException("Rate limit exceeded");

        try
        {
            return await action();
        }
        finally
        {
            // ReSharper disable once MethodSupportsCancellation
            // delay should be applied in any case
            await Task.Delay(Delay);
        }
    }

    public void Dispose()
    {
        limiter.Dispose();
    }
}