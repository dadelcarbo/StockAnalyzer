using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_EMATRAIL : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }
        public override string Definition => "Paint cloud based on EMA TRAIL like";

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

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var emaSerie = closeSerie.CalculateEMA(period);

            var bullSerie = new FloatSerie(stockSerie.Count);
            var bearSerie = new FloatSerie(stockSerie.Count);

            var min = emaSerie.GetMin(0, period - 1);
            var max = emaSerie.GetMax(0, period - 1);
            for (int i = 0; i < period; i++)
            {
                bullSerie[i] = max;
                bearSerie[i] = min;
            }
            var isBull = closeSerie[period - 1] > emaSerie[period - 1];
            for (int i = period; i < stockSerie.Count; i++)
            {
                min = emaSerie.GetMin(i - period + 1, i);
                max = emaSerie.GetMax(i - period + 1, i);

                if (isBull)
                {
                    if (closeSerie[i] < min) // Break down
                    {
                        bullSerie[i] = min;
                        bearSerie[i] = max;
                        isBull = false;
                    }
                    else
                    {
                        bullSerie[i] = max;
                        bearSerie[i] = min;
                    }
                }
                else
                {
                    if (closeSerie[i] > max) // Break up
                    {
                        bullSerie[i] = max;
                        bearSerie[i] = min;
                        isBull = true;
                    }
                    else
                    {
                        bullSerie[i] = min;
                        bearSerie[i] = max;
                    }
                }
            }

            this.Series[0] = bullSerie;
            this.Series[1] = bearSerie;
            this.Series[2] = (bullSerie + bearSerie) / 2.0f;

            // Detecting events
            this.GenerateEvents(stockSerie);
        }
    }
}
