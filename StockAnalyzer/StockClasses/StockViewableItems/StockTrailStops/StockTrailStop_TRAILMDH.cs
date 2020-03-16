using System;
using System.Drawing;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILMDH : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "UpRatio", "DownRatio" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 30, 1.0f, -1.0f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 2.0f), new ParamRangeFloat(-2.0f, 0.0f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILMDH.S", "TRAILMDH.R" }; } }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            var period = (int)this.parameters[0];

            IStockIndicator indicator = stockSerie.GetIndicator($"MDH({period})");

            var midSerie = indicator.Series[1];

            var diffSerie = (indicator.Series[0] - midSerie);
            var upSerie = midSerie + diffSerie * (float)this.parameters[1];
            diffSerie = (indicator.Series[2] - midSerie);
            var lowSerie = midSerie - diffSerie * (float)this.parameters[2];

            stockSerie.CalculateBBTrailStop(lowSerie, upSerie, out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
