using System.Threading;
using System.Threading.Tasks;

namespace YahooFinanceApi;

public sealed partial class Yahoo
{
    public static async Task<SecurityProfile> QueryProfileAsync(string symbol, CancellationToken token = default) 
    {
        var pascalDictionary = await QuerySummaryAsync(symbol, token, SummaryModules.AssetProfile);
        return new SecurityProfile(pascalDictionary);
    }
}