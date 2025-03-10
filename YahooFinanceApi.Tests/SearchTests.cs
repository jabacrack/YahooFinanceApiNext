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
        Assert.NotNull(result.Score);
        Assert.Equal("EQUITY", result.Type);
    }

    [Fact]
    public async Task IsinSearchTest()
    {
        //AMD
        var searchResults = await Yahoo.SearchAsync("US0079031078", 100);

        var result = searchResults.FirstOrDefault();

        Assert.NotNull(result);
        Assert.Equal("AMD", result.Symbol);
        Assert.Equal("EQUITY", result.Type);
    }
}