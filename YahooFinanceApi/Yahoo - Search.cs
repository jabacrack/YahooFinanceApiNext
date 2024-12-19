using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;

namespace YahooFinanceApi;

public sealed partial class Yahoo
{
    public static async Task<SearchResult[]> SearchAsync(string query, int resultsCount = 20, CancellationToken token = default)
    {
        var url = $"https://query2.finance.yahoo.com/v1/finance/search";

        dynamic data;
        
        try
        {
            data = await url
                .WithHeader(YahooSession.UserAgentKey, YahooSession.UserAgentValue)
                .WithHeader("Referer", "https://finance.yahoo.com/lookup")
                .SetQueryParam("q", query)
                .SetQueryParam("quotesCount", resultsCount)
                .GetAsync(token)
                .ReceiveJson()
                .ConfigureAwait(false);
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == (int)HttpStatusCode.NotFound)
        {
            return null;
        }

        IList<dynamic> quotes = data.quotes;

        var results = quotes
            .Cast<IDictionary<string, dynamic>>()
            .Select(CreateSearchResult)
            .ToArray();
        
        return results;

        SearchResult CreateSearchResult(IDictionary<string, dynamic> response)
        {
            dynamic rawScore = response.GetValueOrDefault("score");
            int? score = rawScore == null ? null : Convert.ToInt32(rawScore);

            return new SearchResult(
                symbol: response.GetValueOrDefault("symbol"),
                exchange: response.GetValueOrDefault("exchange"),
                shortName: response.GetValueOrDefault("shortname"),
                longName: response.GetValueOrDefault("longname"),
                sector: response.GetValueOrDefault("sector"),
                industry: response.GetValueOrDefault("industry"),
                type: response.GetValueOrDefault("quoteType"),
                score: score
            );
        }
    }
}