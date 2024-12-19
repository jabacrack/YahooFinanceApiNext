namespace YahooFinanceApi;

public record SearchResult
{
    public SearchResult(string symbol, string exchange, string shortName, string longName, string sector, string industry, string type, int? score)
    {
        Symbol = symbol;
        Exchange = exchange;
        ShortName = shortName;
        LongName = longName;
        Sector = sector;
        Industry = industry;
        Type = type;
        Score = score;
    }

    public string Symbol { get; }
    public string Exchange { get; }
    public string ShortName { get; }
    public string LongName { get; }
    public string Sector { get; }
    public string Industry { get; }
    public string Type { get; }
    public int? Score { get; }
}
