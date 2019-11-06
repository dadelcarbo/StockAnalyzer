using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILPERCENT : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "BullTrigger", "BearTrigger" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 0.05f, 0.05f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeFloat(0.0f, 1f), new ParamRangeFloat(0.0f, 1f) }; }
        }
        public override string[] SerieNames { get { return new string[] { "TRAILPERCENT.LS", "TRAILPERCENT.SS" }; } }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count);
            
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            var lowTriggerRatio = 1 + (float)this.parameters[0];
            var highTriggerRatio = 1 - (float)this.parameters[1];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie LowSerie = stockSerie.GetSerie(StockDataType.LOW);



            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
