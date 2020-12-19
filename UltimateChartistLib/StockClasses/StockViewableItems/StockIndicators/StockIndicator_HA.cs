using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HA : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { }; }
        }

        public override string[] SerieNames { get { return new string[] { "HA" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var ha = stockSerie.GetIndicator("HEIKINASHI()");
            var haDiff = ha.Series[3] - ha.Series[0];

            this.series[0] = haDiff;
            this.Series[0].Name = this.SerieNames[0];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool upTrend = true, previousUpTrend = true;

            for (int i = 1; i < stockSerie.Count; i++)
            {
                previousUpTrend = upTrend;
                upTrend = haDiff[i] > 0;
                int count = 0;
                this.Events[count++][i] = upTrend;
                this.Events[count++][i] = !upTrend;
                this.Events[count++][i] = upTrend && !previousUpTrend;
                this.Events[count++][i] = !upTrend && previousUpTrend;
            }
        }

        static string[] eventNames = new string[]
          {
            "Uptrend", "DownTrend", "BrokenUp","BrokenDown"
          };

        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[]
          {
            false, false, true, true
          };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
