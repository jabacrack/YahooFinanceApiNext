using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace YahooFinanceApi.Tests;

public class SearchTests
{
    [Fact]
    public async Task SearchAsyncTest()
    {
        var searchResults = await Yahoo.SearchAsync("microsoft", 100);

        var result = searchResults.FirstOrDefault(x => x.Symbol == "MSFT");
        
        Assert.NotNull(result);
        Assert.NotNull(result.Exchange);
        Assert.NotNull(result.Industry);
        Assert.NotNull(result.Sector);
        Assert.NotNull(result.LongName);
        Assert.NotNull(result.ShortName);
        Assert.Equal("EQUITY", result.Type);
    }
}