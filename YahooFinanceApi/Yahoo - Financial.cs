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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using YahooFinanceApi.Data;
using YahooFinanceApi.Enums;

namespace YahooFinanceApi;

public sealed partial class Yahoo
{
    public static async Task<Dictionary<string, FinancialPoint[]>> QueryFinancialTableAsync(string symbol, FinancialTable table,
        FinancialFrequency frequency, CancellationToken token = default)
    {
        await YahooSession.InitAsync(token);

        var keys = Constants.FinancialKeys[table];
        var timeScale = frequency switch
        {
            FinancialFrequency.Yearly => "annual",
            FinancialFrequency.Quarterly => "quarterly",
            // FinancialFrequency.Trailing => "trailing",
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null)
        };

        // Yahoo returns maximum 4 years or 5 quarters, regardless of start_dt:
        var startTime = new DateTime(2016, 12, 31);
        var endTime = DateTime.UtcNow.Date;

        var data = await GetResponseStreamAsync(RequestData);
        var dataList = data as IList<dynamic>;
        if (dataList == null)
            return null;

        var result = new Dictionary<string, FinancialPoint[]>();

        foreach (var record in dataList)
        {
            var dict = record as IDictionary<string, dynamic>;
            if (dict == null)
                continue;

            var key = dict.Keys.Except(["timestamp", "meta"]).FirstOrDefault();
            if (key == null)
                continue;

            var name = key.StartsWith(timeScale) ? key.Substring(timeScale.Length) : key;

            var points = dict[key] as IList<dynamic>;
            if (points == null)
                continue;

            result[name] = points
                .Select(x => new FinancialPoint(DateTime.ParseExact(x.asOfDate, "yyyy-MM-dd", CultureInfo.InvariantCulture), x.reportedValue?.raw,
                    x.currencyCode))
                .OrderBy(x => x.Date)
                .ToArray();
        }

        return result;

        async Task<dynamic> RequestData()
        {
            var yahooKeys = string.Join(",", keys.Select(x => $"{timeScale}{x}"));

            var url = "https://query2.finance.yahoo.com/ws/fundamentals-timeseries/v1/finance/timeseries/"
                .AppendPathSegment(symbol)
                .SetQueryParam("period1", startTime.ToUnixTimestamp())
                .SetQueryParam("period2", endTime.ToUnixTimestamp())
                .SetQueryParam("symbol", symbol)
                .SetQueryParam("type", yahooKeys)
                .SetQueryParam("crumb", YahooSession.Crumb);

            var response = await url
                .WithCookie(YahooSession.Cookie.Name, YahooSession.Cookie.Value)
                .WithHeader(YahooSession.UserAgentKey, YahooSession.UserAgentValue)
                .GetAsync(token);

            var json = await response.GetJsonAsync();
            var error = json.timeseries?.error?.description;
            if (error != null)
            {
                throw new InvalidDataException($"An error was returned by Yahoo: {error}");
            }

            return json.timeseries?.result;
        }
    }
}