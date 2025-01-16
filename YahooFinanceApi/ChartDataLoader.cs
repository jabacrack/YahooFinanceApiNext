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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace YahooFinanceApi;

public static class ChartDataLoader
{
    public static async Task<dynamic> GetChartDataAsync(this YahooSession session, string symbol, DateTime startTime, DateTime endTime, Period period, string events, CancellationToken cancellationToken)
    {
        return await session.DoRequest((addAuth, token) => GetResponseStreamAsync(addAuth, symbol, startTime, endTime, period, events, token), cancellationToken).ConfigureAwait(false);
    }
    
    private static async Task<dynamic> GetResponseStreamAsync(Func<Url, IFlurlRequest> addAuth, string symbol, DateTime startTime, DateTime endTime, Period period, string events, CancellationToken token)
    {
        var url = "https://query2.finance.yahoo.com/v8/finance/chart/"
            .AppendPathSegment(symbol)
            .SetQueryParam("period1", startTime.ToUnixTimestamp())
            .SetQueryParam("period2", endTime.ToUnixTimestamp())
            .SetQueryParam("interval", $"1{period.Name()}")
            .SetQueryParam("events", events);

        var response = await addAuth(url).GetAsync(token).ConfigureAwait(false);
        var json = await response.GetJsonAsync().ConfigureAwait(false);

        var error = json.chart?.error?.description;
        if (error != null)
        {
            throw new InvalidDataException($"An error was returned by Yahoo: {error}");
        }
                
        return json;
    }
}