using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILVOL : StockTrailStopBase
    {
        public StockTrailStop_TRAILVOL()
        {
        }
        public override string Name
        {
            get { return "TRAILVOL(" + this.Parameters[0].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "TRAILVOL(bool TrailGap)"; }
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return true; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "TrailGap"}; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { true }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeBool() }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILVOL.S", "TRAILVOL.R" }; } }
        
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
            stockSerie.CalculateVolumeTrailStop((bool)this.Parameters[0], out longStopSerie, out shortStopSerie);
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
        static readonly bool[] isEvent = new bool[] { false, false, true, true, true, true};
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
