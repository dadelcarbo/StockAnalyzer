using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILATRLONG : StockTrailStopBase
    {
        public override string Definition => "Draws Trail Stop based ATR Bands for long only trades";
        public override string[] ParameterNames => new string[] { "Period", "ATRPeriod", "NbUpDev", "NbDownDev", "MAType", "ReentryPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 30, 10, 2f, -2f, "EMA", 6 };
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA(),
                new ParamRangeInt(0, 500)
                };
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            var emaSerie = stockSerie.GetIndicator(this.parameters[4] + "(" + (int)parameters[0] + ")").Series[0];
            var atrSerie = stockSerie.GetIndicator("ATR(" + (int)parameters[1] + ")").Series[0];

            var upDev = (float)parameters[2];
            var downDev = (float)parameters[3];
            var upperBand = emaSerie + upDev * atrSerie;
            var lowerBand = emaSerie + downDev * atrSerie;

            stockSerie.CalculateBandTrailStop(lowerBand, upperBand, out longStopSerie, out shortStopSerie);

            this.Series[0] = longStopSerie;
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = new FloatSerie(stockSerie.Count, float.NaN);
            this.Series[1].Name = this.SerieNames[1];

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}
