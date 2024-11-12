// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// This code is derived from yfinance (https://github.com/ranaroussi/yfinance)

namespace YahooFinanceApi;

public class SummaryModules
{
    /// <summary>
    /// Contains general information about the company
    /// </summary>
    public const string SummaryProfile = "summaryProfile";
    /// <summary>
    /// Prices + volume + market cap + etc
    /// </summary>
    public const string SummaryDetail = "summaryDetail";
    /// <summary>
    /// <see cref="SummaryProfile"/> + company officers
    /// </summary>
    public const string AssetProfile = "assetProfile"; 
    public const string FundProfile = "fundProfile";
    /// <summary>
    /// Current prices
    /// </summary>
    public const string Price = "price"; 
    /// <summary>
    /// Quote type
    /// </summary>
    public const string QuoteType = "quoteType";
    /// <summary>
    /// Environmental, social, and governance (ESG) scores, sustainability and ethical performance of companies
    /// </summary>
    public const string EsgScores = "esgScores"; 
    public const string IncomeStatementHistory = "incomeStatementHistory";
    public const string IncomeStatementHistoryQuarterly = "incomeStatementHistoryQuarterly";
    public const string BalanceSheetHistory = "balanceSheetHistory";
    public const string BalanceSheetHistoryQuarterly = "balanceSheetHistoryQuarterly";
    public const string CashFlowStatementHistory = "cashFlowStatementHistory";
    public const string CashFlowStatementHistoryQuarterly = "cashFlowStatementHistoryQuarterly";
    /// <summary>
    /// KPIs (PE, enterprise value, EPS, EBITA, and more)
    /// </summary>
    public const string DefaultKeyStatistics = "defaultKeyStatistics";
    /// <summary>
    /// Financial KPIs (revenue, gross margins, operating cash flow, free cash flow, and more)
    /// </summary>
    public const string FinancialData = "financialData";
    /// <summary>
    /// Future earnings date
    /// </summary>
    public const string CalendarEvents = "calendarEvents";
    /// <summary>
    /// SEC filings, such as 10K and 10Q reports
    /// </summary>
    public const string SecFilings = "secFilings"; 
    /// <summary>
    /// Upgrades and downgrades that analysts have given a company's stock
    /// </summary>
    public const string UpgradeDowngradeHistory = "upgradeDowngradeHistory";
    /// <summary>
    /// Institutional ownership, holders and shares outstanding
    /// </summary>
    public const string InstitutionOwnership = "institutionOwnership"; 
    /// <summary>
    /// Mutual fund ownership, holders and shares outstanding
    /// </summary>
    public const string FundOwnership = "fundOwnership"; 
    public const string MajorDirectHolders = "majorDirectHolders";
    public const string MajorHoldersBreakdown = "majorHoldersBreakdown";
    /// <summary>
    /// Insider transactions, such as the number of shares bought and sold by company executives
    /// </summary>
    public const string InsiderTransactions = "insiderTransactions"; 
    /// <summary>
    /// Insider holders, such as the number of shares held by company executives
    /// </summary>
    public const string InsiderHolders = "insiderHolders"; 
    /// <summary>
    /// Net share purchase activity, such as the number of shares bought and sold by company executives
    /// </summary>
    public const string NetSharePurchaseActivity = "netSharePurchaseActivity"; 
    /// <summary>
    /// Earnings history
    /// </summary>
    public const string Earnings = "earnings"; 
    public const string EarningsHistory = "earningsHistory";
    /// <summary>
    /// Earnings trend
    /// </summary>
    public const string EarningsTrend = "earningsTrend"; 
    public const string IndustryTrend = "industryTrend";
    public const string IndexTrend = "indexTrend";
    public const string SectorTrend = "sectorTrend";
    public const string RecommendationTrend = "recommendationTrend";
    public const string FuturesChain = "futuresChain";
}