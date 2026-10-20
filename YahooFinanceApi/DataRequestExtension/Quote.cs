using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using YahooFinanceApi.Utils;

namespace YahooFinanceApi.DataRequestExtension;

public static partial class DataRequestExtension
{
    public static async Task<IReadOnlyDictionary<string, Security>> QueryAsync(this YahooSession session, string[] symbols, string[] fields,
        CancellationToken token = default)
    {
        if (symbols == null || symbols.Length == 0)
        {
            throw new InvalidOperationException("Symbols must be set before this method is called.");
        }

        var duplicateSymbol = symbols.Duplicates().FirstOrDefault();
        if (duplicateSymbol != null)
        {
            throw new InvalidOperationException($"Symbols contain a duplicate: {duplicateSymbol}.");
        }

        var url = "https://query1.finance.yahoo.com/v7/finance/quote".SetQueryParam("symbols", string.Join(",", symbols));

        if (fields.Length > 0)
        {
            var duplicateField = fields.Duplicates().FirstOrDefault();
            if (duplicateField != null)
            {
                throw new InvalidOperationException($"Fields contain a duplicate: {duplicateField}.");
            }

            url = url.SetQueryParam("fields", string.Join(",", fields.Select(s => s.ToLowerCamel())));
        }

        // Invalid symbols as part of a request are ignored by Yahoo.
        // So the number of symbols returned may be less than requested.
        // If there are no valid symbols, an exception is thrown by Flurl.
        // This exception is caught (below) and an empty dictionary is returned.
        // There seems to be no easy way to reliably identify changed symbols.

        dynamic data = null;

        try
        {
            data = await url
                .GetAsync(token)
                .ReceiveJson()
                .ConfigureAwait(false);
        }
        catch (FlurlHttpException ex) when (ex.Call.Response?.StatusCode == (int) HttpStatusCode.NotFound)
        {
            return null;
        }

        var response = data.quoteResponse;

        var error = response.error;
        if (error != null)
        {
            throw new InvalidDataException($"An error was returned by Yahoo: {error}");
        }

        var securities = new Dictionary<string, Security>();

        foreach (IDictionary<string, dynamic> map in response.result)
        {
            // Change the Yahoo field names to start with upper case.
            var pascalDictionary = map.ToDictionary(x => x.Key.ToPascal(), x => x.Value);
            securities.Add(pascalDictionary["Symbol"], new Security(pascalDictionary));
        }

        return securities;
    }
}