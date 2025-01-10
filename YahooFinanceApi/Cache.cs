// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// This code is derived from yfinance (https://github.com/ranaroussi/yfinance)

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;

namespace YahooFinanceApi;

public static class Cache
{
    private static readonly Dictionary<string, TimeZoneInfo> timeZoneCache = new ();

    public static async Task<TimeZoneInfo> GetTimeZone(string ticker, CancellationToken cancellationToken)
    {
        if (timeZoneCache.TryGetValue(ticker, out var cachedTimeZone))
            return cachedTimeZone;

        await YahooSession.InitCrumb(cancellationToken);
        
        TimeZoneInfo timeZone = await RequestTimeZone(ticker, cancellationToken);
        if (timeZone != null)
            timeZoneCache[ticker] = timeZone;
        
        return timeZone;
    }

    private static async Task<TimeZoneInfo> RequestTimeZone(string ticker, CancellationToken cancellationToken)
    {
        var startTime = DateTime.Now.AddDays(-2);
        var endTime = DateTime.Now;

        dynamic data = null;
        
        try
        {
            data = await ChartDataLoader.GetResponseStreamAsync(ticker, startTime, endTime, Period.Daily, ShowOption.History.Name(), cancellationToken).ConfigureAwait(false);
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == (int)HttpStatusCode.NotFound)
        {
            return null;
        }
        
        if (data == null)
            return null;
        
        string timeZoneName = data.chart.result[0].meta.exchangeTimezoneName;
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.Utc;
        }
    }
}