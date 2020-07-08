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
            int slowPeriod = (int)this.parameters[1];
            int fastPeriod = (int)this.parameters[0];

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            // Calculate MID line 
            FloatSerie fastLine = new FloatSerie(stockSerie.Count);
            FloatSerie slowLine = new FloatSerie(stockSerie.Count);

            float upLine = highSerie[0];
            float downLine = lowSerie[0];
            slowLine[0] = fastLine[0] = (upLine + downLine) / 2.0f;

            for (int i = 1; i < stockSerie.Count; i++)
            {
                upLine = highSerie.GetMax(Math.Max(0, i - fastPeriod - 1), i - 1);
                downLine = lowSerie.GetMin(Math.Max(0, i - fastPeriod - 1), i - 1);
                fastLine[i] = (upLine + downLine) / 2.0f;

                upLine = highSerie.GetMax(Math.Max(0, i - slowPeriod - 1), i - 1);
                downLine = lowSerie.GetMin(Math.Max(0, i - slowPeriod - 1), i - 1);
                slowLine[i] = (upLine + downLine) / 2.0f;
            }

            this.Series[0] = fastLine;
            this.Series[1] = slowLine;

            // Detecting events
            this.GenerateEvents(stockSerie);
        }
    }
}
