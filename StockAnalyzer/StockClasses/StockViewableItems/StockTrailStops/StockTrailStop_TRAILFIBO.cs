using System;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILFIBO : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override string[] ParameterNames => new string[] { "Period", "Ratio" };

        public override Object[] ParameterDefaultValues => new Object[] { 60, 0.61f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 5f) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, float.NaN);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            int period = (int)this.parameters[0];
            float upRatio = (float)this.parameters[1];

            var indicator = stockSerie.GetIndicator("FIBOCHANNEL(" + period + "," + upRatio + ")");
            var bandUp = indicator.Series[0];
            var fiboUp = indicator.Series[1];
            var fiboDown = indicator.Series[3];
            var bandDown = indicator.Series[4];


            bool upTrend = false;
            bool downTrend = false;
            for (int i = 0; i < period; i++)
            {
                longStopSerie[i] = float.NaN;
                shortStopSerie[i] = float.NaN;
            }
            for (int i = period; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1])
                    { // Trailing stop has been broken => stop up trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = Math.Max(longStopSerie[i - 1], fiboUp[i]);
                    }
                }
                else
                {
                    if (bandUp[i] > bandUp[i - 1])
                    {  // Up trend Starts
                        upTrend = true;
                        longStopSerie[i] = fiboUp[i];
                    }
                }
                if (downTrend)
                {
                    if (closeSerie[i] > shortStopSerie[i - 1])
                    { // Trailing stop has been broken => stop up trend
                        downTrend = false;
                        shortStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        // Trail the stop  
                        shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], fiboDown[i]);
                    }
                }
                else
                {
                    if (bandDown[i] < bandDown[i - 1])
                    {  // Down trend Starts
                        downTrend = true;
                        shortStopSerie[i] = fiboDown[i];
                    }
                }
            }

            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }

    }
}