using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_ALLIGATOR : StockIndicatorBase
    {
        public StockIndicator_ALLIGATOR()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "FiboRank" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3 }; }
        }

        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[] { new ParamRangeInt(1, 10) };
            }
        }

        public override string[] SerieNames
        {
            get
            {
                int fiboRank = (int) this.Parameters[0];
                return new string[]
                {
                    "EMA(" + fibos[fiboRank + 1].ToString() + ")",
                    "EMA(" + fibos[fiboRank + 2].ToString().ToString() + ")",
                    "EMA(" + fibos[fiboRank + 3].ToString().ToString() + ")"
                };
            }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Blue), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }

        static int [] fibos = new int[] {1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233};
  
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie fastSerie = stockSerie.GetIndicator(this.SerieNames[0]).Series[0];
            FloatSerie mediumSerie = stockSerie.GetIndicator(this.SerieNames[1]).Series[0];
            FloatSerie slowSerie = stockSerie.GetIndicator(this.SerieNames[2]).Series[0];
            
            int fiboRank = (int) this.Parameters[0]; 
            int shift = fibos[fiboRank];
            fastSerie = fastSerie.ShiftForward(shift);

            shift = fibos[fiboRank + 1];
            mediumSerie = mediumSerie.ShiftForward(shift);
           
            shift = fibos[fiboRank + 2];
            slowSerie = slowSerie.ShiftForward(shift);

            this.Series[0] = fastSerie;
            this.Series[1] = mediumSerie;
            this.Series[2] = slowSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                if (fastSerie[i] > mediumSerie[i] && mediumSerie[i] > slowSerie[i])
                {
                    this.Events[0][i] = true;
                }
                else if (fastSerie[i] < mediumSerie[i] && mediumSerie[i] < slowSerie[i])
                {
                    this.Events[1][i] = true;
                }
            }
        }

        static string[] eventNames = new string[] { "UpTrend", "DownTrend" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
