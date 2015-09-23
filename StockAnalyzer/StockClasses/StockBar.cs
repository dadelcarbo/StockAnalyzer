using System;

namespace StockAnalyzer.StockClasses
{
    public class StockBar
    {
        public enum StockBarType
        {
            Daily = 0,
            Intraday,
            Range,
            Tick
        };
        public DateTime DATE { get; set; }
        public double OPEN { get; set; }
        public double HIGH { get; set; }
        public double LOW { get; set; }
        public double CLOSE { get; set; }
        public long VOLUME { get; set; }
        public long UPVOLUME { get; set; }
        public int TICK{ get; set; }
        public int UPTICK{ get; set; }
        public bool Complete { get; set; }

        public StockBar(double open, double high, double low, double close, long volume, long upVolume, int tick, int upTick, DateTime date)
        {
            this.DATE = date;
            this.OPEN = open;
            this.HIGH = high;
            this.LOW = low;
            this.CLOSE = close;
            this.VOLUME = volume;
            this.UPVOLUME = upVolume;
            this.TICK = tick;
            this.UPTICK = upTick;
            this.Complete = true;
        }
        public StockBar(string line)
        {
            string []fields = line.Split(',');
            this.DATE = DateTime.Parse(fields[0]);
            this.OPEN = double.Parse(fields[1]);
            this.HIGH = double.Parse(fields[2]);
            this.LOW = double.Parse(fields[3]);
            this.CLOSE = double.Parse(fields[4]);
            this.VOLUME = long.Parse(fields[5]);
            this.UPVOLUME = long.Parse(fields[6]);
            this.TICK = int.Parse(fields[7]);
            this.UPTICK = int.Parse(fields[8]);
            this.Complete = true;
        }

        #region CSV file IO
        static public string StringFormat()
        {
            return "Date,Open,High,Low,Close,Volume,Adj Close,UpVolume,Tick,UpTick";
        }
        public override string ToString()
        {
            return DATE.ToString("s") + "," + OPEN.ToString() + "," + HIGH.ToString() + "," + LOW.ToString() + "," + CLOSE.ToString()
                + "," + VOLUME.ToString() + "," + CLOSE.ToString() + "," + UPVOLUME.ToString() + "," + TICK.ToString() + "," + UPTICK.ToString();
        }
        #endregion
        //public override bool Equals(object obj)
        //{
        //    StockBar otherBar = (StockBar)obj;
        //    return this.DATE == otherBar.DATE && this.OPEN == otherBar.OPEN && this.HIGH == otherBar.HIGH && this.LOW == otherBar.LOW && this.CLOSE == otherBar.CLOSE
        //        && this.VOLUME == otherBar.VOLUME && this.UPVOLUME == otherBar.UPVOLUME && this.TICK == otherBar.TICK && this.UPTICK == otherBar.UPTICK;
        //}
    }
}
