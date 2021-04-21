using System;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses
{
    public enum BarDuration
    {
        Daily,
        Weekly,
        Monthly,
        M_5,
        M_15,
        M_30,
        H_1,
        H_2,
        H_4,
        Bar_2,
        Bar_3,
        Bar_6,
        Bar_9,
        Bar_12,
        Bar_24,
        Bar_27,
        Bar_48,
        RENKO_2
    }

    public class StockBarDuration : IComparable
    {
        public BarDuration Duration { get; set; }
        public int Smoothing { get; set; }
        public int LineBreak { get; set; }

        public bool HeikinAshi { get; set; }

        public StockBarDuration()
        {
            this.Duration = BarDuration.Daily;
            this.Smoothing = 1;
            this.LineBreak = 0;
            this.HeikinAshi = false;
        }
        public StockBarDuration(BarDuration duration)
        {
            this.Duration = duration;
            this.Smoothing = 1;
            this.LineBreak = 0;
            this.HeikinAshi = false;
        }
        public StockBarDuration(BarDuration duration, int smoothing, bool heikinAshi = false, int lineBreak = 0)
        {
            this.Duration = duration;
            this.Smoothing = smoothing;
            this.HeikinAshi = heikinAshi;
            this.LineBreak = lineBreak;
        }
        public StockBarDuration(StockBarDuration duration)
        {
            this.Duration = duration.Duration;
            this.Smoothing = duration.Smoothing;
            this.HeikinAshi = duration.HeikinAshi;
            this.LineBreak = duration.LineBreak;
        }

        public static StockBarDuration Daily = new StockBarDuration(BarDuration.Daily);
        public static StockBarDuration H_1 = new StockBarDuration(BarDuration.H_1);
        public static StockBarDuration H_2 = new StockBarDuration(BarDuration.H_2);
        public static StockBarDuration H_4 = new StockBarDuration(BarDuration.H_4);
        public static StockBarDuration Bar_3 = new StockBarDuration(BarDuration.Bar_3);
        public static StockBarDuration Bar_6 = new StockBarDuration(BarDuration.Bar_6);
        public static StockBarDuration Bar_12 = new StockBarDuration(BarDuration.Bar_12);
        public static StockBarDuration Bar_24 = new StockBarDuration(BarDuration.Bar_24);
        public static StockBarDuration Bar_48 = new StockBarDuration(BarDuration.Bar_48);
        public static StockBarDuration Weekly = new StockBarDuration(BarDuration.Weekly);
        public static StockBarDuration Monthly = new StockBarDuration(BarDuration.Monthly);

        public static IList<StockBarDuration> Values = new List<StockBarDuration>() {
            StockBarDuration.Daily,
            StockBarDuration.Weekly,
            StockBarDuration.Monthly,
            StockBarDuration.H_1,
            StockBarDuration.H_2,
            StockBarDuration.H_4,
            StockBarDuration.Bar_3,
            StockBarDuration.Bar_6,
            StockBarDuration.Bar_12,
            StockBarDuration.Bar_24,
            StockBarDuration.Bar_48,
        };

        internal static bool TryParse(string v, out StockBarDuration barDuration)
        {
            BarDuration duration;
            if (Enum.TryParse<BarDuration>(v, out duration))
            {
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
            var str = this.Smoothing <= 1 ? this.Duration.ToString() : this.Duration + "_EMA" + this.Smoothing;
            str += this.LineBreak == 0 ? string.Empty : "_LB" + this.LineBreak;
            return this.HeikinAshi ? str + "_HA" : str;
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
                return (Duration == p.Duration) && (Smoothing == p.Smoothing) && (HeikinAshi == p.HeikinAshi) && (LineBreak == p.LineBreak);
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