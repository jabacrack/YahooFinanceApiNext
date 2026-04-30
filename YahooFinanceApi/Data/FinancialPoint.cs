using System;

namespace YahooFinanceApi.Data;

public class FinancialPoint(DateTime date, double value, string currency)
{
    public DateTime Date { get; } = date;
    public double Value { get; } = value;
    public string Currency { get; } = currency;
}