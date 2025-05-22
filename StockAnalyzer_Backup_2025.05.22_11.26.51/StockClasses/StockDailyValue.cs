using System;
using System.Globalization;
using System.IO;

namespace StockAnalyzer.StockClasses
{
    public enum StockDataType
    {
        CLOSE = 0,
        OPEN,
        HIGH,
        LOW,
        BODYHIGH,
        BODYLOW,
        /// <summary>
        /// Variation in percent from previous bar
        /// </summary>
        VARIATION,
        /// <summary>
        /// ATR: Average True Range, includes gap from previous bar
        /// </summary>
        ATR,
        /// <summary>
        /// ADR: Average Bar Range, ignores gap from previous bar
        /// </summary>
        ADR,
        /// <summary>
        /// ADBR: Average Bar Body Range
        /// </summary>
        ADBR,
        VOLUME,
        /// <summary>
        /// Amount of currency exchange during the bar. 0.5f * (OPEN + CLOSE) * VOLUME; 
        /// </summary>
        EXCHANGED
    };

    public class StockDailyValue
    {
        public DateTime DATE { get; set; }
        public float OPEN { get; set; }
        public float HIGH { get; set; }
        public float LOW { get; set; }
        public float CLOSE { get; set; }
        /// <summary>
        /// Exchanged volume expressed in number of shares.
        /// </summary>
        public long VOLUME { get; set; }
        /// <summary>
        /// Exchanged capital expressed in serie currency.
        /// </summary>
        public float EXCHANGED => 0.5f * (OPEN + CLOSE) * VOLUME;
        public float VARIATION { get; set; }

        public float BodyHigh => Math.Max(OPEN, CLOSE);
        public float BodyLow => Math.Min(OPEN, CLOSE);

        public float ADBR => BodyHigh - BodyLow;
        public float NADBR => ADBR / CLOSE;

        public float ADR => HIGH - LOW;
        public float NADR => ADR / CLOSE;

        public float ATR { get; set; }
        public float NATR => ATR / CLOSE;

        private static readonly CultureInfo usCulture = CultureInfo.GetCultureInfo("en-US");

        public float GetStockData(StockDataType dataType)
        {
            if (dataType == StockDataType.VOLUME)
            {
                return (float)this.VOLUME;
            }
            Type type = this.GetType();
            System.Reflection.PropertyInfo propInfo = type.GetProperty(dataType.ToString());

            return (float)propInfo.GetValue(this, null);
        }
        public StockDailyValue(DateTime date, StockDailyValue source)
        {
            this.DATE = date;
            this.OPEN = source.OPEN;
            this.HIGH = source.HIGH;
            this.LOW = source.LOW;
            this.CLOSE = source.CLOSE;
            this.VOLUME = source.VOLUME;
        }
        public StockDailyValue(float open, float high, float low, float close, long volume, DateTime date)
        {
            this.DATE = date;
            if (open == 0.0f)
            {
                this.OPEN = close;
            }
            else
            {
                this.OPEN = open;
            }
            this.HIGH = Math.Max(Math.Max(Math.Max(high, this.OPEN), close), low);
            if (low == 0.0f)
            {
                this.LOW = Math.Min(this.OPEN, close);
            }
            else
            {
                this.LOW = Math.Min(Math.Min(Math.Min(low, this.OPEN), close), this.HIGH);
            }
            this.CLOSE = close;
            this.VOLUME = volume;
        }

        #region CSV file IO
        public static StockDailyValue ReadMarketDataFromCSVStream(StreamReader sr, string stockName, bool useAdjusted)
        {
            StockDailyValue stockValue = null;
            try
            {
                // File format
                // Date,Open,High,Low,Close,Volume,Adj Close (UpVolume, Tick, Uptick)
                // 2010-06-18,10435.00,10513.75,10379.60,10450.64,4555360000,10450.64
                string[] row = sr.ReadLine().Split(',');
                if (row.Length == 7)
                {
                    if (useAdjusted || row[4] != row[6])
                    {
                        float close = float.Parse(row[4], usCulture);
                        float adjClose = float.Parse(row[6], usCulture);
                        float adjRatio = adjClose / close;
                        stockValue = new StockDailyValue(
                            float.Parse(row[1], usCulture) * adjRatio,
                            float.Parse(row[2], usCulture) * adjRatio,
                            float.Parse(row[3], usCulture) * adjRatio,
                            adjClose,
                            long.Parse(row[5], usCulture),
                            DateTime.Parse(row[0], usCulture));
                    }
                    else
                    {
                        stockValue = new StockDailyValue(
                                float.Parse(row[1], usCulture),
                                float.Parse(row[2], usCulture),
                                float.Parse(row[3], usCulture),
                                float.Parse(row[4], usCulture),
                                long.Parse(row[5], usCulture),
                                DateTime.Parse(row[0], usCulture));
                    }
                }
                if (row.Length == 6)
                {
                    stockValue = new StockDailyValue(
                            float.Parse(row[1], usCulture),
                            float.Parse(row[2], usCulture),
                            float.Parse(row[3], usCulture),
                            float.Parse(row[4], usCulture),
                            long.Parse(row[5], usCulture),
                            DateTime.Parse(row[0], usCulture));
                }
                else if (row.Length == 10)
                {
                    stockValue = new StockDailyValue(
                                float.Parse(row[1], usCulture),
                                float.Parse(row[2], usCulture),
                                float.Parse(row[3], usCulture),
                                float.Parse(row[4], usCulture),
                                long.Parse(row[5], usCulture),
                                DateTime.Parse(row[0], usCulture));
                }
            }
            catch (Exception)
            {
                // Assume input is right, Ignore invalid lines
            }
            return stockValue;
        }

        static public string StringFormat()
        {
            return "Date,Open,High,Low,Close,Volume,Adj Close";
        }

        public override string ToString()
        {
            return DATE.ToString("s") + "," + OPEN.ToString(usCulture) + "," + HIGH.ToString(usCulture) + "," + LOW.ToString(usCulture) + "," + CLOSE.ToString(usCulture) + "," + VOLUME.ToString(usCulture);
        }

        internal void ApplyRatio(float ratio)
        {
            this.OPEN *= ratio;
            this.HIGH *= ratio;
            this.LOW *= ratio;
            this.CLOSE *= ratio;
            this.VOLUME = (long)(this.VOLUME / ratio);
        }
        #endregion

        private bool isComplete = true;
        public bool IsComplete { get { return isComplete; } set { isComplete = value; } }
    }
}
