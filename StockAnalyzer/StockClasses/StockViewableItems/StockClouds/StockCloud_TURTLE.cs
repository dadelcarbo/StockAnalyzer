using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_TURTLE : StockCloudBase
    {
        public override string Definition => base.Definition + Environment.NewLine + "Cloud based on steroïd turtles as defined by InvestingZen.";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string[] ParameterNames => new string[] { "HighPeriod", "LowPeriod", "EMAPeriod" };
        public override Object[] ParameterDefaultValues => new Object[] { 36, 12, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "High", "Low", "EMA" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            var indicator = stockSerie.GetIndicator(this.Name);
            var upLine = indicator.Series[0];
            var emaSerie = indicator.Series[2];

            this.Series[0] = indicator.Series[0];
            this.Series[1] = indicator.Series[1];
            this.Series[2] = indicator.Series[2];

            // Detecting events
            this.GenerateEvents(stockSerie);
        }
    }
}
