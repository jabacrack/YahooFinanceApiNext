using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace YahooFinanceApi.Tests;

public class CacheTests
{
    [Fact]
    public async Task GetTimeZone_TickerNotFound_ReturnNull()
    {
        var timeZone = await Cache.GetTimeZone("BRK.B", CancellationToken.None);
        Assert.Null(timeZone);
    }
}