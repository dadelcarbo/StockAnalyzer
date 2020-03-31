using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_MDH : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string Definition => "Paint a cloud base on two EMA lines";
        public override string[] ParameterNames
        {
            get { return new string[] { "FastPeriod", "SlowPeriod" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 50 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 1), new Pen(Color.DarkRed, 1) };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie bullSerie = stockSerie.GetIndicator($"MDH({(int)this.parameters[0]})").Series[1];
            FloatSerie bearSerie = stockSerie.GetIndicator($"MDH({(int)this.parameters[1]})").Series[1];

            this.Series[0] = bullSerie;
            this.Series[1] = bearSerie;

            // Detecting events
            this.GenerateEvents(stockSerie);
        }
    }
}
