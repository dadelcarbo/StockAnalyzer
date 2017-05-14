using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HILBERTTREND : StockIndicatorBase
    {
        public StockIndicator_HILBERTTREND()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.NonRangedIndicator; }
        }
        public float Max
        {
            get { return 1.0f; }
        }

        public float Min
        {
            get { return -1.0f; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Smoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "HILBERTTREND" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Black) { DashStyle = System.Drawing.Drawing2D.DashStyle.Custom } };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var hilbertIndicator = stockSerie.GetIndicator(this.Name.Replace("TREND", ""));
            FloatSerie trendSerie = hilbertIndicator.Series[0] - hilbertIndicator.Series[1];

            //var stockastikSerie = stockSerie.GetIndicator("STOKS(30,3,3,75,25)").Series[0];
            //stockastikSerie = (stockastikSerie - 50f) / 50f;
            trendSerie = trendSerie;

            this.series[0] = trendSerie;

            this.SetSerieNames();


            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 10; i < trendSerie.Count; i++)
            {
                this.eventSeries[0][i] = trendSerie[i] > 0;
                this.eventSeries[1][i] = trendSerie[i] < 0;
                this.eventSeries[2][i] = trendSerie[i] > 0 && trendSerie[i - 1] < 0;
                this.eventSeries[3][i] = trendSerie[i] < 0 && trendSerie[i - 1] > 0;
            }
        }

        static string[] eventNames = new string[] { "UpSwing", "DownSwing", "BullishCrossing", "BearishCrossing" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
