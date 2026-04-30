using System.Threading.Tasks;
using Xunit;
using YahooFinanceApi.Enums;

namespace YahooFinanceApi.Tests;

public class FinancialTests
{
    [Fact]
    public async Task QueryFinancialTableAsync()
    {
        const string AAPL = "AAPL";

        await Yahoo.QueryFinancialTableAsync(AAPL, FinancialTable.Income, FinancialFrequency.Quarterly);
    }
}