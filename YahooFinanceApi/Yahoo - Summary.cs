using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace YahooFinanceApi
{
    public sealed partial class Yahoo
    {

        public static async Task<IReadOnlyDictionary<string, dynamic>> QuerySummaryAsync(string symbol, CancellationToken token = default, params string[] modules)
        {
            if (modules.Length == 0)
                throw new ArgumentException("Select at least one module");
            
            await YahooSession.InitAsync(token);

            var modulesString = string.Join(",", modules);
            
            var url = $"https://query2.finance.yahoo.com/v10/finance/quoteSummary/{symbol}"
                .SetQueryParam("modules", modulesString)
                .SetQueryParam("formatted", false)
                .SetQueryParam("crumb", YahooSession.Crumb);

            // Invalid symbols as part of a request are ignored by Yahoo.
            // So the number of symbols returned may be less than requested.
            // If there are no valid symbols, an exception is thrown by Flurl.
            // This exception is caught (below) and an empty dictionary is returned.
            // There seems to be no easy way to reliably identify changed symbols.

            dynamic data = null;

            try
            {
                data = await url
                    .WithCookie(YahooSession.Cookie.Name, YahooSession.Cookie.Value)
                    .WithHeader(YahooSession.UserAgentKey, YahooSession.UserAgentValue)
                    .GetAsync(token)
                    .ReceiveJson()
                    .ConfigureAwait(false);
            }
            catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == (int)HttpStatusCode.NotFound)
            {
                return new Dictionary<string, dynamic>();
            }

            var response = data.quoteSummary;

            var error = response.error;
            if (error != null)
            {
                throw new InvalidDataException($"An error was returned by Yahoo: {error}");
            }

            var results = new Dictionary<string, dynamic>();
            
            foreach (var module in modules)
            {
                IDictionary<string, dynamic> moduleResult = ((IDictionary<string, dynamic>)response.result[0]).GetValueOrDefault(module);
                
                if (moduleResult != null)
                {
                    foreach (var pair in moduleResult)
                    {
                        var value = GetValue(pair.Value);
                        if (value == null)
                            continue;
                        
                        results[pair.Key.ToPascal()] = value;
                    }
                }
            }

            return results;

            dynamic GetValue(dynamic raw)
            {
                var dict = raw as IDictionary<string, dynamic>;

                if (dict == null)
                    return raw;

                if (dict.Count == 0)
                    return null;
                
                if (dict.TryGetValue("raw", out var value))
                    return value;
                
                return raw;
            }
        }
    }
}
