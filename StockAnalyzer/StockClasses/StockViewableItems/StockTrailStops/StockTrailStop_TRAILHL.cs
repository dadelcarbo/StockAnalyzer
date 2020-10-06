using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILHL : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 2 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "TRAILHL.LS", "TRAILHL.SS" };
        
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            stockSerie.CalculateHighLowTrailStop((int)this.Parameters[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[0].Name = longStopSerie.Name;
            this.Series[1] = shortStopSerie;
            this.Series[1].Name = shortStopSerie.Name;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}