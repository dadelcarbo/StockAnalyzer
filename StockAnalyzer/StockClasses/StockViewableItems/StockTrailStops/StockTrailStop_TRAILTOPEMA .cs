using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILTOPEMA : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is calculated as a EMA starting from the previous extremum.";

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
        public override string[] SerieNames { get { return new string[] { "TRAILTOPEMA.LS", "TRAILTOPEMA.SS" }; } }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var topEMA = stockSerie.GetIndicator($"TOPEMA({(int)this.Parameters[0]})");
            FloatSerie supportSerie = topEMA.Series[0];
            FloatSerie resistanceSerie = topEMA.Series[1];
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILTOPEMA.LS", float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILTOPEMA.SS", float.NaN);
            int i = 0;
            while (++i < stockSerie.Count && !topEMA.Events[6][i] && !topEMA.Events[7][i]) ;
            bool bullish = topEMA.Events[6][i];
            for (; i < stockSerie.Count; i++)
            {
                if (bullish)
                {
                    if (topEMA.Events[7][i])
                    {
                        bullish = false;
                        shortStopSerie[i] = resistanceSerie[i];
                    }
                    else
                    {
                        longStopSerie[i] = supportSerie[i];
                    }
                }
                else
                {
                    if (topEMA.Events[6][i])
                    {
                        bullish = true;
                        longStopSerie[i] = supportSerie[i];
                    }
                    else
                    {
                        shortStopSerie[i] = resistanceSerie[i];
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
