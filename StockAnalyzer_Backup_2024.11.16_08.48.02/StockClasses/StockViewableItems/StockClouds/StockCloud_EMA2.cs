using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_EMA2 : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string Definition => "Paint a cloud base on two EMA lines";
        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 50 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "Bull", "Bear" };
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie bullSerie = stockSerie.GetIndicator($"EMA({(int)this.parameters[0]})").Series[0];
            FloatSerie bearSerie = stockSerie.GetIndicator($"EMA({(int)this.parameters[1]})").Series[0];

            this.Series[0] = bullSerie;
            this.Series[1] = bearSerie;

            // Detecting events
            base.GenerateEvents(stockSerie);
        }
    }
}
