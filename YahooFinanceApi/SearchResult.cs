namespace YahooFinanceApi;

public record SearchResult
{
    public SearchResult(string symbol, string exchange, string shortName, string longName, string sector, string industry, string type)
    {
        Symbol = symbol;
        Exchange = exchange;
        ShortName = shortName;
        LongName = longName;
        Sector = sector;
        Industry = industry;
        Type = type;
    }

    public string Symbol { get; }
    public string Exchange { get; }
    public string ShortName { get; }
    public string LongName { get; }
    public string Sector { get; }
    public string Industry { get; }
    public string Type { get; }
}
