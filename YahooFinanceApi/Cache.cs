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
using System.Threading;
using System.Threading.Tasks;

namespace YahooFinanceApi;

public static class Cache
{
    private static Dictionary<string, TimeZoneInfo> timeZoneCache = new Dictionary<string, TimeZoneInfo>();

    public static async Task<TimeZoneInfo> GetTimeZone(string ticker)
    {
        if (timeZoneCache.TryGetValue(ticker, out var zone))
            return zone;

        var timeZone = await RequestTimeZone(ticker);
        timeZoneCache[ticker] = timeZone;
        return timeZone;
    }

    private static async Task<TimeZoneInfo> RequestTimeZone(string ticker)
    {
        var startTime = DateTime.Now.AddDays(-2);
        var endTime = DateTime.Now;
        var data = await ChartDataLoader.GetResponseStreamAsync(ticker, startTime, endTime, Period.Daily, ShowOption.History.Name(), CancellationToken.None);
        var timeZoneName = data.chart.result[0].meta.exchangeTimezoneName;
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneName);
        }
        catch (TimeZoneNotFoundException e)
        {
            return TimeZoneInfo.Utc;
        }
    }
}