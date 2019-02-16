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
            get { return new string[] { "TrailGap" }; }
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
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;
            stockSerie.CalculateVolumeTrailStop((bool)this.Parameters[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}