using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILHL2 : StockTrailStopBase
    {
        public StockTrailStop_TRAILHL2()
        {
        }
        public override string Definition
        {
            get { return "TRAILHL2(int period)"; }
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 2 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(0, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILHL.LS", "TRAILHL.SS" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;
            stockSerie.CalculateHighLowTrailStop2((int)this.Parameters[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 5; i < stockSerie.Count; i++)
            {
                this.Events[0][i] = !float.IsNaN(longStopSerie[i]);
                this.Events[1][i] = !float.IsNaN(shortStopSerie[i]);
                this.Events[2][i] = float.IsNaN(longStopSerie[i - 1]) && !float.IsNaN(longStopSerie[i]);
                this.Events[3][i] = float.IsNaN(shortStopSerie[i - 1]) && !float.IsNaN(shortStopSerie[i]);
                this.Events[4][i] = !float.IsNaN(longStopSerie[i - 1]) && !float.IsNaN(longStopSerie[i]) && longStopSerie[i - 1] < longStopSerie[i];
                this.Events[5][i] = !float.IsNaN(shortStopSerie[i - 1]) && !float.IsNaN(shortStopSerie[i]) && shortStopSerie[i - 1] > shortStopSerie[i];
            }
        }

        static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BrokenUp", "BrokenDown", "TrailedUp", "TrailedDown" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
