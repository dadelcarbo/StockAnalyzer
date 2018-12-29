using System;

namespace StockAnalyzer.StockClasses
{
    public enum BarDuration
    {
        Daily,
        Weekly,
        Monthly,
        Bar_2,
        Bar_3,
        Bar_6,
        Bar_9,
        Bar_12,
        Bar_24,
        Bar_27,
        Bar_48,
        HA,
        HA_3D,
        MIN_5,
        MIN_15,
        MIN_60,
        MIN_120,
        TLB,
        TLB_3D,
        TLB_6D,
        TLB_9D,
        TLB_27D,
        ThreeLineBreak,
        SixLineBreak,
        TLB_Weekly
    }

    public class StockBarDuration : IComparable
    {
        public BarDuration Duration { get; set; }
        public int Smoothing { get; set; }

        public StockBarDuration()
        {
            this.Duration = BarDuration.Daily;
            this.Smoothing = 1;
        }
        public StockBarDuration(BarDuration duration)
        {
            this.Duration = duration;
            this.Smoothing = 1;
        }
        public StockBarDuration(BarDuration duration, int smoothing)
        {
            this.Duration = duration;
            this.Smoothing = smoothing;
        }

        public static StockBarDuration Daily = new StockBarDuration(BarDuration.Daily);
        public static StockBarDuration TLB = new StockBarDuration(BarDuration.TLB);
        public static StockBarDuration TLB_3D = new StockBarDuration(BarDuration.TLB_3D);
        public static StockBarDuration TLB_6D = new StockBarDuration(BarDuration.TLB_6D);
        public static StockBarDuration TLB_9D = new StockBarDuration(BarDuration.TLB_9D);
        public static StockBarDuration Bar_3 = new StockBarDuration(BarDuration.Bar_3);
        public static StockBarDuration Bar_6 = new StockBarDuration(BarDuration.Bar_6);
        public static StockBarDuration Bar_12 = new StockBarDuration(BarDuration.Bar_12);
        public static StockBarDuration Weekly = new StockBarDuration(BarDuration.Weekly);
        public static StockBarDuration Monthly = new StockBarDuration(BarDuration.Monthly);

        internal static bool TryParse(string v, out StockBarDuration barDuration)
        {
            BarDuration duration;
            if (Enum.TryParse<BarDuration>(v, out duration)) {
                barDuration = duration;
                return true;
            }
            barDuration = StockBarDuration.Daily;
            return false;
        }

        public static bool operator ==(StockBarDuration o1, StockBarDuration o2)
        {
            if (object.ReferenceEquals(o1, null))
            {
                return object.ReferenceEquals(o2, null);
            }

            return o1.Equals(o2);
        }
        public static bool operator !=(StockBarDuration o1, StockBarDuration o2)
        {
            return !(o1 == o2);
        }

        public override string ToString()
        {
            return this.Smoothing <= 1 ? this.Duration.ToString() : this.Duration + "_EMA" + this.Smoothing;
        }
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                StockBarDuration p = (StockBarDuration)obj;
                return (Duration == p.Duration) && (Smoothing == p.Smoothing);
            }
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public int CompareTo(object obj)
        {
            return Equals(obj) ? 0 : -1;
        }

        public static implicit operator StockBarDuration(BarDuration barDuration)
        {
            return new StockBarDuration(barDuration);
        }
    }
}