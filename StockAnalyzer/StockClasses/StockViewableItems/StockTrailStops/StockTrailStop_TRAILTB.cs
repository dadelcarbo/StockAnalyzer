using System;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILTB : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                };
            }
        }
        public override string[] SerieNames { get { return new string[] { "TRAILTB.S", "TRAILTB.R" }; } }

        public override void ApplyTo(StockSerie stockSerie)
        {
            IStockIndicator tbIndicator = stockSerie.GetIndicator(this.Name.Replace("TRAIL", ""));

            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;

            stockSerie.CalculateBBTrailStop(tbIndicator.Series[1], tbIndicator.Series[0], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
