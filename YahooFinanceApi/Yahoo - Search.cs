using System.Collections.Generic;
using System.Linq;
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
        catch (FlurlHttpException ex)
        {
            if (ex.Call.Response.StatusCode == (int)System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                throw;
            }
        }

        IList<dynamic> quotes = data.quotes;

        var results = quotes
            .Cast<IDictionary<string, dynamic>>()
            .Select(CreateSearchResult)
            .ToArray();
        
        return results;

        SearchResult CreateSearchResult(IDictionary<string, dynamic> response)
        {
            return new SearchResult(
                symbol: response.GetValueOrDefault(key: "symbol"),
                exchange: response.GetValueOrDefault(key: "exchange"),
                shortName: response.GetValueOrDefault(key: "shortname"),
                longName: response.GetValueOrDefault(key: "longname"),
                sector: response.GetValueOrDefault(key: "sector"),
                industry: response.GetValueOrDefault(key: "industry"),
                type: response.GetValueOrDefault(key: "quoteType")
            );
        }
    }
}