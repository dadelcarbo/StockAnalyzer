using System;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;

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
        /// Variation in percent from previous bar (Close to Close)
        /// </summary>
        VARIATION,
        /// <summary>
        /// ADR: Average Bar Range, ignores gap from previous bar
        /// </summary>
        ADR,
        /// <summary>
        /// ADR: Average Bar Range, ignores gap from previous bar
        /// </summary>
        ATR,
        /// <summary>
        /// Represents the number of exchanged stocks.
        /// </summary>
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

        /// <summary>
        /// ADR Average Day Range (no gap)
        /// </summary>
        public float ADR => HIGH - LOW;
        public float NADR => ADR / CLOSE;

        public bool IsComplete { get; set; } = true;


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


        internal void ApplyRatio(float ratio)
        {
            this.OPEN *= ratio;
            this.HIGH *= ratio;
            this.LOW *= ratio;
            this.CLOSE *= ratio;
            this.VOLUME = (long)(this.VOLUME / ratio);
        }

    }
}
