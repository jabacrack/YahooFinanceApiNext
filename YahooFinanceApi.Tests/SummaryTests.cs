using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace YahooFinanceApi.Tests;

public class SummaryTests
{
    [Fact]
    public async Task TestSummaryAsync()
    {
        var summary = await Yahoo.QuerySummaryAsync("MSFT", default, SummaryModules.AssetProfile, SummaryModules.DefaultKeyStatistics);
        long? freeFloat = summary.GetValueOrDefault("FloatShares");
        
        Assert.NotNull(freeFloat);
    }
    
    [Fact]
    public async Task QuerySummaryAsync_ModuleNotPresent_IgnoreMissedModule()
    {
        var summary = await Yahoo.QuerySummaryAsync("^SPX", default, SummaryModules.AssetProfile, SummaryModules.QuoteType);
    }
}