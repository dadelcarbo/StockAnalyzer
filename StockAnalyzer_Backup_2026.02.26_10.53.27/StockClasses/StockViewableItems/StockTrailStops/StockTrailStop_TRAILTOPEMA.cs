using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILTOPEMA : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Draw a trail stop based on TOPEMA.";

        public override string[] ParameterNames => new string[] { "UpPeriod", "DownPeriod", "Smoothing" };

        public override Object[] ParameterDefaultValues => new Object[] { 175, 35, 4 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var topEMA = stockSerie.GetIndicator($"TOPEMA({this.Parameters[0]},{this.Parameters[1]},{this.Parameters[2]})");
            FloatSerie supportSerie = topEMA.Series[0];
            FloatSerie resistanceSerie = topEMA.Series[1];
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILTOPEMA.LS", float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILTOPEMA.SS", float.NaN);
            int i = 0;
            while (++i < stockSerie.Count && !topEMA.Events[8][i] && !topEMA.Events[8][i]) ;
            for (; i < stockSerie.Count; i++)
            {
                if (topEMA.Events[9][i]) // Bearish
                {
                    shortStopSerie[i] = resistanceSerie[i];
                }
                else
                {
                    longStopSerie[i] = supportSerie[i];
                }
            }

            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
