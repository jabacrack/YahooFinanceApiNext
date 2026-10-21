using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using YahooFinanceApi.Utils;

namespace YahooFinanceApi.DataRequestExtensions;

public static class HistoricalExtensions
{
    public static async Task<IReadOnlyList<Candle>> GetHistoricalAsync(this YahooSession session, string symbol,
        DateTime? startTime = null, DateTime? endTime = null, Period period = Period.Daily, CancellationToken token = default)
        => await GetTicksAsync(session, symbol, startTime, endTime, period, ShowOption.History, DataConvertors.ToCandle, token).ConfigureAwait(false);

    public static async Task<IReadOnlyList<DividendTick>> GetDividendsAsync(this YahooSession session, string symbol,
        DateTime? startTime = null, DateTime? endTime = null, CancellationToken token = default)
        => await GetTicksAsync(session, symbol, startTime, endTime, Period.Daily, ShowOption.Dividend, DataConvertors.ToDividendTick, token).ConfigureAwait(false);

    public static async Task<IReadOnlyList<SplitTick>> GetSplitsAsync(this YahooSession session, string symbol,
        DateTime? startTime = null, DateTime? endTime = null, CancellationToken token = default)
        => await GetTicksAsync(session, symbol, startTime, endTime, Period.Daily, ShowOption.Split, DataConvertors.ToSplitTick, token).ConfigureAwait(false);

    private static async Task<List<T>> GetTicksAsync<T>(this YahooSession session, string symbol, DateTime? startTime, DateTime? endTime, Period period,
        ShowOption showOption, Func<ExpandoObject, TimeZoneInfo, List<T>> converter, CancellationToken token)
        where T : ITick
    {
        dynamic json = await session.DoRequest(ctx => GetData(symbol, startTime, endTime, period, showOption.Name(), ctx), token).ConfigureAwait(false);

        if (json == null)
            return new List<T>();

        dynamic data = json.chart.result[0];

        List<T> allData = converter(data, symbolTimeZone);
        return allData.Where(x => x != null).Where(x => x.DateTime <= endTime.Value).ToList();
    }

    private static async Task<dynamic> GetData(this YahooSession session, string symbol, DateTime? startTime, DateTime? endTime, Period period, string events,
        CancellationToken cancellationToken)
    {
        TimeZoneInfo symbolTimeZone = await session.GetTimeZone(symbol, cancellationToken).ConfigureAwait(false);

        if (symbolTimeZone == null)
            return null;

        await session.SleepBetweenRequests(cancellationToken).ConfigureAwait(false);

        startTime ??= Helper.Epoch;
        endTime ??= DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        DateTime start = startTime.Value.ToUtcFrom(symbolTimeZone);
        DateTime end = endTime.Value.AddDays(2).ToUtcFrom(symbolTimeZone);

        try
        {
            return await session.GetChartDataAsync(symbol, start, end, period, events, cancellationToken).ConfigureAwait(false);
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == (int) HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}