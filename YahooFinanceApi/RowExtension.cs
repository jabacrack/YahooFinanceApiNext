using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;

namespace YahooFinanceApi
{
    internal static class RowExtension
    {
        internal static bool IgnoreEmptyRows;

        // internal static Candle ToCandle(DateTime date, IDictionary<string, object> row)
        internal static List<Candle> ToCandle(dynamic data)
        {
            List<object> timestamps = data.timestamp;
            DateTime[] dates = timestamps.Select(x => DateTimeOffset.FromUnixTimeSeconds((long)x).Date).ToArray();
            IDictionary<string, object> indicators = data.indicators;
            IDictionary<string, object> values = data.indicators.quote[0];

            if (indicators.ContainsKey("adjclose"))
                values["adjclose"] = data.indicators.adjclose[0].adjclose;

            var ticks = new List<Candle>();
            
            for (int i = 0; i < dates.Length; i++)
            {
                var slice = new Dictionary<string, object>();
                foreach (KeyValuePair<string, object> pair in values)
                {
                    List<object> ts = (List<object>) pair.Value;
                    slice.Add(pair.Key, ts[i]);
                }
                ticks.Add(CreateCandle(dates[i], slice));
            }

            return ticks;
            
            Candle CreateCandle(DateTime date, IDictionary<string, object> row)
            {
                var candle = new Candle
                {
                    DateTime      = date,
                    Open          = row["open"].ToDecimal(),
                    High          = row["high"].ToDecimal(),
                    Low           = row["low"].ToDecimal(),
                    Close         = row["close"].ToDecimal(),
                    AdjustedClose = row["adjclose"].ToDecimal(),
                    Volume        = row["volume"].ToInt64()
                };

                if (IgnoreEmptyRows &&
                    candle.Open == 0 && candle.High == 0 && candle.Low == 0 && candle.Close == 0 &&
                    candle.AdjustedClose == 0 &&  candle.Volume == 0)
                    return null;

                return candle;
            }
        }

        internal static List<DividendTick> ToDividendTick(dynamic data)
        {
            IDictionary<string, object> expandoObject = data;

            if (!expandoObject.ContainsKey("events"))
                return new List<DividendTick>();
            
            IDictionary<string, dynamic> dvdObj = data.events.dividends;
            var dividends = dvdObj.Values.Select(x => new DividendTick(((object) x.date).ToDateTime(), ((object) x.amount).ToDecimal())).ToList();

            if (IgnoreEmptyRows)
                dividends = dividends.Where(x => x.Dividend > 0).ToList();

            return dividends;
            
            
            return null;
            
            // var tick = new DividendTick
            // {
            //     DateTime = date,
            //     Dividend = row["dvd"].ToDecimal()
            // };
            //
            // if (IgnoreEmptyRows && tick.Dividend == 0)
            //     return null;
            //
            // return tick;
        }

        internal static List<SplitTick> ToSplitTick(dynamic data)
        {
            IDictionary<string, dynamic> splitsObj = data.events.splits;
            var splits = splitsObj.Values.Select(x => new SplitTick(((object) x.date).ToDateTime(), ((object) x.numerator).ToDecimal(), ((object) x.denominator).ToDecimal())).ToList();
            
            if (IgnoreEmptyRows)
                splits = splits.Where(x => x.BeforeSplit > 0 && x.AfterSplit > 0).ToList();
            
            return splits;
            
            // var tick = new SplitTick { DateTime = date };

            //TODO
            // var split = row[1].Split(':');
            // if (split.Length == 2)
            // {
            //     tick.BeforeSplit = split[0].ToDecimal();
            //     tick.AfterSplit  = split[1].ToDecimal();
            // }
            //
            // if (IgnoreEmptyRows && tick.AfterSplit == 0 && tick.BeforeSplit == 0)
            //     return null;

            // return tick;
        }

        private static DateTime ToDateTime(this object str)
        {
            if (str is long lng)
            {
                return DateTimeOffset.FromUnixTimeSeconds(lng).Date;
            }

            throw new Exception($"Could not convert '{str}' to DateTime.");
        }

        private static Decimal ToDecimal(this object obj)
        {
            return Convert.ToDecimal(obj);
        }

        private static Int64 ToInt64(this object obj)
        {
            return Convert.ToInt64(obj);
        }
    }
}
