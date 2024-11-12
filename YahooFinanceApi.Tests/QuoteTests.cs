using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace YahooFinanceApi.Tests;

public class QuotesTest
{
    private string[] tickers = ["ABBN.SW", "ACLN.SW", "ADEN.SW", "ADVN.SW", "ADXN.SW", "AERO.SW", "AEVS.SW", "AIRE.SW", "ALC.SW", "ALLN.SW", "ALSN.SW", "AMS.SW", "APGN.SW", "ARBN.SW", "ARON.SW", "ARYN.SW", "ASCN.SW", "ASWN.SW", "AUTN.SW", "AVOL.SW", "BAER.SW", "BALN.SW", "BANB.SW", "BARN.SW", "BBN.SW", "BCGE.SW", "BCHN.SW", "BCJ.SW", "BCVN.SW", "BEAN.SW", "BEKN.SW", "BELL.SW", "BION.SW", "BKW.SW", "BLKB.SW", "BOSN.SW", "BRKN.SW", "BSKP.SW", "BSLN.SW", "BUCN.SW", "BVZN.SW", "BYS.SW", "CALN.SW", "CFR.SW", "CFT.SW", "CICN.SW", "CIE.SW", "CLN.SW", "CLTN.SW", "CMBN.SW", "COPN.SW", "COTN.SW", "CPHN.SW", "CURN.SW", "DAE.SW", "DESN.SW", "DKSH.SW", "DOCM.SW", "DOKA.SW", "EFGN.SW", "EMMN.SW", "EMSN.SW", "EPIC.SW", "ESUN.SW", "EVE.SW", "FHZN.SW", "FORN.SW", "FREN.SW", "FTON.SW", "GALD.SW", "GALE.SW", "GAM.SW", "GAV.SW", "GEBN.SW", "GF.SW", "GIVN.SW", "GLKBN.SW", "GMI.SW", "GRKP.SW", "GURN.SW", "HBLN.SW", "HELN.SW", "HIAG.SW", "HLEE.SW", "HOCN.SW", "HOLN.SW", "HUBN.SW", "IDIA.SW", "IFCN.SW", "IMPN.SW", "INA.SW", "INRN.SW", "IREN.SW", "ISN.SW", "JFN.SW", "KARN.SW", "KLIN.SW", "KNIN.SW", "KOMN.SW", "KUD.SW", "KURN.SW", "LAND.SW", "LECN.SW", "LEHN.SW", "LEON.SW", "LISN.SW", "LISP.SW", "LLBN.SW", "LMN.SW", "LOGN.SW", "LONN.SW", "LUKN.SW", "MBTN.SW", "MCHN.SW", "MED.SW", "MEDX.SW", "METN.SW", "MIKN.SW", "MOBN.SW", "MOLN.SW", "MOVE.SW", "MOZN.SW", "MTG.SW", "NESN.SW", "NOVN.SW", "NREN.SW", "NWRN.SW", "ODHN.SW", "OERL.SW", "OFN.SW", "ORON.SW", "PEAN.SW", "PEDU.SW", "PEHN.SW", "PGHN.SW", "PKTM.SW", "PLAN.SW", "PMN.SW", "PPGN.SW", "PSPN.SW", "REHN.SW", "RIEN.SW", "RLF.SW", "RO.SW", "ROG.SW", "RSGN.SW", "SANN.SW", "SCHN.SW", "SCHP.SW", "SCMN.SW", "SDZ.SW", "SENS.SW", "SFPN.SW", "SFSN.SW", "SFZN.SW", "SGKN.SW", "SGSN.SW", "SHLTN.SW", "SIGN.SW", "SIKA.SW", "SKAN.SW", "SLHN.SW", "SNBN.SW", "SOON.SW", "SPEX.SW", "SPSN.SW", "SQN.SW", "SRAIL.SW", "SREN.SW", "STGN.SW", "STLN.SW", "STMN.SW", "STRN.SW", "SUN.SW", "SWON.SW", "SWTQ.SW", "TECN.SW", "TEMN.SW", "TIBN.SW", "TKBP.SW", "TXGN.SW", "UBSG.SW", "UBXN.SW", "UHR.SW", "UHRN.SW", "VACN.SW", "VAHN.SW", "VARN.SW", "VATN.SW", "VBSN.SW", "VETN.SW", "VILN.SW", "VONN.SW", "VPBN.SW", "VZN.SW", "VZUG.SW", "WARN.SW", "WIHN.SW", "WKBN.SW", "XLS.SW", "YPSN.SW", "ZEHN.SW", "ZUBN.SW", "ZUGER.SW", "ZUGN.SW", "ZURN.SW", "ZWM.SW"];
    
    [Fact]
    public async Task TestQuoteAsync()
    {
        const string AAPL = "AAPL";

        var quote = await Yahoo.Symbols(AAPL).Fields(
                Field.RegularMarketOpen,
                Field.RegularMarketDayHigh,
                Field.RegularMarketDayLow,
                Field.RegularMarketPrice,
                Field.RegularMarketVolume,
                Field.RegularMarketTime)
            .QueryAsync();

        var aaplQuote = quote[AAPL];
        var aaplOpen = aaplQuote[Field.RegularMarketOpen.ToString()];
        var aaplHigh = aaplQuote[Field.RegularMarketDayHigh.ToString()];
        var aaplLow = aaplQuote[Field.RegularMarketDayLow.ToString()];
        var aaplCurrentPrice = aaplQuote[Field.RegularMarketPrice.ToString()];
        var aaplVolume = aaplQuote[Field.RegularMarketVolume.ToString()];
        var aaplTime = aaplQuote[Field.RegularMarketTime.ToString()];

        // Get New York Timezone for conversion from UTC to New York Time for Yahoo Quotes
        TimeZoneInfo TzEst = TimeZoneInfo
            .GetSystemTimeZones()
            .Single(tz => tz.Id == "Eastern Standard Time" || tz.Id == "America/New_York");

        long unixDate = 1568232001; // Any unix timestamp
        DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime date = start.AddSeconds(unixDate);
        DateTime estDate = TimeZoneInfo.ConvertTimeFromUtc(date, TzEst);
    }

    [Fact]
    public async Task TestSymbolsArgument()
    {
        // one symbols
        var securities = await Yahoo.Symbols("C").QueryAsync();
        Assert.Single(securities);

        // no symbols
        await Assert.ThrowsAsync<ArgumentException>(async () => await Yahoo.Symbols().QueryAsync());

        // duplicate symbol
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await Yahoo.Symbols("C", "A", "C").QueryAsync());

        // invalid symbols are ignored by Yahoo!
        securities = await Yahoo.Symbols("invalidsymbol").QueryAsync();
        Assert.Empty(securities);

        securities = await Yahoo.Symbols("C", "invalidsymbol", "X").QueryAsync();
        Assert.Equal(2, securities.Count);
    }

    [Fact]
    public async Task TestFieldsArgument()
    {
        // when no fields are specified, many(all?) fields are returned!
        var securities = await Yahoo.Symbols("C").QueryAsync();
        Assert.True(securities["C"].Fields.Count > 10);

        // duplicate field
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await Yahoo.Symbols("C").Fields("currency", "bid").Fields(Field.Ask, Field.Bid).QueryAsync());

        // invalid fields are ignored
        securities = await Yahoo.Symbols("C").Fields("invalidfield").QueryAsync();
        var security = securities["C"];
        Assert.Throws<KeyNotFoundException>(() => security["invalidfield"]);
        Assert.False(security.Fields.TryGetValue("invalidfield", out dynamic bid));
    }

    [Fact]
    public async Task TestQuery()
    {
        var securities = await Yahoo
            .Symbols("C", "AAPL")
            // Can use string field names:
            .Fields("Bid", "Ask", "Tradeable", "LongName")
            // and/or field enums:
            .Fields(Field.RegularMarketPrice, Field.Currency)
            .QueryAsync();

        Assert.Equal(2, securities.Count);
        var security = securities["C"];

        // Bid string or enum indexer returns dynamic type.
        security.Fields.TryGetValue("Bid", out dynamic bid);
        bid = security.Fields["Bid"];
        bid = security["Bid"];
        bid = security[Field.Bid];

        // Bid property returns static type.
        var bid2 = security.Bid;

        Assert.False(securities["C"][Field.Tradeable]);
        Assert.Equal("Apple Inc.", securities["AAPL"]["LongName"]);
    }
    
    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    public async Task SeveralSymbols(int count)
    {
        var subTickers = tickers.Take(count).ToArray();
        
        var securities = await Yahoo
            .Symbols(subTickers)
            // Can use string field names:
            .Fields("Bid", "Ask", "Tradeable", "LongName")
            // and/or field enums:
            .Fields(Field.RegularMarketPrice, Field.Currency)
            .QueryAsync();

        Assert.Equal(count, securities.Count);
    }
}