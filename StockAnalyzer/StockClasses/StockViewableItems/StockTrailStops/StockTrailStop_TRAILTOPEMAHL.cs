using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILTOPEMAHL : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is starting at first TOPEMA HigherLow and trailed.";

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
            get { return new Object[] { 30 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILTOPEMA.LS", float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILTOPEMA.SS", float.NaN);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var topEMA = stockSerie.GetIndicator($"TOPEMA({(int)this.Parameters[0]})");
            FloatSerie supportSerie = topEMA.Series[0];
            FloatSerie resistanceSerie = topEMA.Series[1];
            BoolSerie hlEvents = topEMA.GetEvents("HigherLow");
            bool bull = false;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (bull)
                {
                    if (closeSerie[i] < longStopSerie[i - 1]) // Up trend boken
                    {
                        bull = false;
                    }
                    else
                    {
                        if (hlEvents[i]) // Trail up
                        {
                            longStopSerie[i] = supportSerie[i];
                        }
                        else
                        {
                            longStopSerie[i] = longStopSerie[i - 1];
                        }
                    }
                }
                else
                {
                    if (hlEvents[i]) // Start up trand
                    {
                        bull = true;
                        longStopSerie[i] = supportSerie[i];
                    }
                }
            }

            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
