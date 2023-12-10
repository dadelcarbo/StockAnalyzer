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
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        public override string Definition => "Paint cloud based on MDH";

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 50 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }
        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green, 1), new Pen(Color.DarkRed, 1), new Pen(Color.DarkBlue, 2) };
                return seriePens;
            }
        }
        public override string[] SerieNames { get { return new string[] { "Bull", "Bear", "Mid" }; } }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var period = (int)this.parameters[0];

            var mdhIndicator = stockSerie.GetIndicator($"MDH({period})");
            var isBullSerie = mdhIndicator.Events[0];

            var bullSerie = new FloatSerie(stockSerie.Count);
            var bearSerie = new FloatSerie(stockSerie.Count);
            var midSerie = mdhIndicator.Series[1];

            for (int i = 0; i < stockSerie.Count; i++)
            {
                if (isBullSerie[i])
                {
                    bullSerie[i] = mdhIndicator.Series[0][i];
                    bearSerie[i] = mdhIndicator.Series[2][i];
                }
                else
                {
                    bullSerie[i] = mdhIndicator.Series[2][i];
                    bearSerie[i] = mdhIndicator.Series[0][i];
                }
            }

            this.Series[0] = bullSerie;
            this.Series[1] = bearSerie;
            this.Series[2] = midSerie;

            // Detecting events
            this.GenerateEvents(stockSerie);
        }
    }
}
