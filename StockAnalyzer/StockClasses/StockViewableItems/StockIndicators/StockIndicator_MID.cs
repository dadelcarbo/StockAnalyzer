using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_MID : StockIndicatorMovingAvgBase
    {
        public override string Definition
        {
            get { return "Calculates the mid point from High and Low over the given period, it can be used as a moving average"; }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.parameters[0];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            // Calculate MID line 
            FloatSerie upLine = new FloatSerie(stockSerie.Count);
            FloatSerie midLine = new FloatSerie(stockSerie.Count);
            FloatSerie downLine = new FloatSerie(stockSerie.Count);

            upLine[0] = closeSerie[0];
            downLine[0] = closeSerie[0];
            midLine[0] = closeSerie[0];

            for (int i = 1; i < stockSerie.Count; i++)
            {
                upLine[i] = highSerie.GetMax(Math.Max(0, i - period - 1), i - 1);
                downLine[i] = lowSerie.GetMin(Math.Max(0, i - period - 1), i - 1);
                midLine[i] = (upLine[i] + downLine[i]) / 2.0f;
            }
            this.series[0] = midLine;
            this.series[0].Name = this.Name;

            // Detecting 
            this.CalculateEvents(stockSerie);
        }
    }
}
