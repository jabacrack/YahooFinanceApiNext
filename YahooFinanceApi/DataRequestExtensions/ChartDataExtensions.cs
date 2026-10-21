using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using YahooFinanceApi.Utils;

namespace YahooFinanceApi.DataRequestExtensions;

internal static class ChartDataExtensions
{
    /// <summary>
    /// This method do not go via rate limiter!
    /// </summary>
    public static async Task<dynamic> GetChartDataAsync(this YahooSession session, string symbol,
        DateTime startTime, DateTime endTime, Period period, string events, CancellationToken cancellationToken)
    {
        var json = await "https://query2.finance.yahoo.com/v8/finance/chart/"
            .AppendPathSegment(symbol)
            .SetQueryParam("period1", startTime.ToUnixTimestamp())
            .SetQueryParam("period2", endTime.ToUnixTimestamp())
            .SetQueryParam("interval", $"1{period.Name()}")
            .SetQueryParam("events", events)
            .AddAuth(session)
            .GetAsync(cancellationToken)
            .ReceiveJson()
            .ConfigureAwait(false);

        var error = json.chart?.error?.description;
        if (error != null)
        {
            throw new InvalidDataException($"An error was returned by Yahoo: {error}");
        }

        return json;
    }
}