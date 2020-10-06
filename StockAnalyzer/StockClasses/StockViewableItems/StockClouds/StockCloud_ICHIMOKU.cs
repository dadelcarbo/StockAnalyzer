using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_ICHIMOKU : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 9, 26, 52 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "TenkanSen", "KijunSen", "SenkouSpanB" }; }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen, 1), new Pen(Color.DarkRed, 1) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            var tenkanSen = (int)this.parameters[0];
            var kijunSen = (int)this.parameters[1];
            var senkouSpanB = (int)this.parameters[2];

            var indicator = stockSerie.GetIndicator($"ICHIMOKU({tenkanSen},{kijunSen},{senkouSpanB})");
            this.Series[0] = indicator.Series[0];
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = indicator.Series[1];
            this.Series[1].Name = this.SerieNames[1];

            // Detecting events
            this.GenerateEvents(stockSerie);
        }
    }
}
