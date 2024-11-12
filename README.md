# YahooFinanceApi - Next

[//]: # ([![build]&#40;https://github.com/lppkarl/yahoofinanceapi/workflows/build/badge.svg&#41;]&#40;https://github.com/lppkarl/YahooFinanceApi/actions?query=workflow%3Abuild&#41;)

[//]: # ([![NuGet]&#40;https://img.shields.io/nuget/v/YahooFinanceApi.svg&#41;]&#40;https://www.nuget.org/packages/YahooFinanceApi/&#41;)

[//]: # ([![NuGet]&#40;https://img.shields.io/nuget/dt/YahooFinanceApi.svg&#41;]&#40;https://www.nuget.org/packages/YahooFinanceApi/&#41;)
[![license](https://img.shields.io/github/license/lppkarl/YahooFinanceApi.svg)](https://github.com/lppkarl/YahooFinanceApi/blob/master/LICENSE)

This library is the successor to the Yahoo! Finance API wrapper: a small tool for retrieving data from Yahoo Finance, written in .NET Standard 2.0.


## Features
* Get quotes
* Get historical data
* Get dividend data
* Get stock split data
* Get the portfolio of a security.

[Changelog](https://github.com/jabacrack/YahooFinanceApi/blob/master/CHANGELOG.md)


## LEGAL DISCLAIMER
YahooFinanceApi - Next is an independent open-source tool, not associated with, endorsed, or verified by Yahoo, Inc. It utilizes Yahoo's publicly available APIs and is meant strictly for personal use.


## Supported Platforms
* .NET Core 2.0
* .NET framework 4.6.1 or above
* Xamarin.iOS
* Xamarin.Android
* Universal Windows Platform


## How To Install
You can find the package through Nuget

    PM> Install-Package YahooFinanceApiNext

### Install Note
For traditional .NET framework user, if you find a "System.Runtime.Serialization.Primitives" missing exception is thrown when using this library, you have to install the missing package manually as nuget does not auto install this reference for you (Bugged?)


## How to Avoid ban from Yahoo
The new API used for historical data is more restrictive than the previous one. The following simple rules allow me to request data without any issues:
* Request only one ticker at a time.
* Wait 1 second between requests.


## How To Use

### Add reference

    using YahooFinanceApi;

### Get stock quotes
    // You could query multiple symbols with multiple fields through the following steps:
    var securities = await Yahoo.Symbols("AAPL", "GOOG").Fields(Field.Symbol, Field.RegularMarketPrice, Field.FiftyTwoWeekHigh).QueryAsync();
    var aapl = securities["AAPL"];
    var price = aapl[Field.RegularMarketPrice] // or, you could use aapl.RegularMarketPrice directly for typed-value

### Supported fields for stock quote
Language, QuoteType, QuoteSourceName, Currency, MarketState, RegularMarketPrice, RegularMarketTime, RegularMarketChange, RegularMarketOpen, RegularMarketDayHigh, RegularMarketDayLow, RegularMarketVolume, ShortName, FiftyTwoWeekHighChange, FiftyTwoWeekHighChangePercent, FiftyTwoWeekLow, FiftyTwoWeekHigh, DividendDate, EarningsTimestamp, EarningsTimestampStart, EarningsTimestampEnd, TrailingAnnualDividendRate, TrailingPE, TrailingAnnualDividendYield, EpsTrailingTwelveMonths, EpsForward, SharesOutstanding, BookValue, RegularMarketChangePercent, RegularMarketPreviousClose, Bid, Ask, BidSize, AskSize, MessageBoardId, FullExchangeName, LongName, FinancialCurrency, AverageDailyVolume3Month, AverageDailyVolume10Day, FiftyTwoWeekLowChange, FiftyTwoWeekLowChangePercent, TwoHundredDayAverageChangePercent, MarketCap, ForwardPE, PriceToBook, SourceInterval, ExchangeTimezoneName, ExchangeTimezoneShortName, Market, Exchange, ExchangeDataDelayedBy, PriceHint, FiftyDayAverage, FiftyDayAverageChange, FiftyDayAverageChangePercent, TwoHundredDayAverage, TwoHundredDayAverageChange, Tradeable, GmtOffSetMilliseconds, Symbol

### Ignore invalid rows
    // Sometimes, yahoo returns broken rows for historical calls, you could decide if these invalid rows is ignored or not by the following statement
    Yahoo.IgnoreEmptyRows = true;

### Get historical data for a stock
    // You should be able to query data from various markets including US, HK, TW
    // The startTime & endTime here defaults to EST timezone
    var history = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1), Period.Daily);

    foreach (var candle in history)
    {
        Console.WriteLine($"DateTime: {candle.DateTime}, Open: {candle.Open}, High: {candle.High}, Low: {candle.Low}, Close: {candle.Close}, Volume: {candle.Volume}, AdjustedClose: {candle.AdjustedClose}");
    }

### Get dividend history for a stock
    // You should be able to query data from various markets including US, HK, TW
    var dividends = await Yahoo.GetDividendsAsync("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1));
    foreach (var candle in dividends)
    {
        Console.WriteLine($"DateTime: {candle.DateTime}, Dividend: {candle.Dividend}");
    }

### Get stock split history for a stock
    var splits = await Yahoo.GetSplitsAsync("AAPL", new DateTime(2014, 6, 8), new DateTime(2014, 6, 10));
    foreach (var s in splits)
    {
        Console.WriteLine($"DateTime: {s.DateTime}, AfterSplit: {s.AfterSplit}, BeforeSplit: {s.BeforeSplit}");
    }


### Powered by
* [Flurl](https://github.com/tmenier/Flurl) ([@tmenier](https://github.com/tmenier)) - A simple & elegant fluent-style REST api library 
