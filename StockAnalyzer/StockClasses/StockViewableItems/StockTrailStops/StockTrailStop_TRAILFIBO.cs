using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILFIBO : StockTrailStopBase
    {
        public StockTrailStop_TRAILFIBO()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override string[] ParameterNames => new string[] { "Period", "Ratio" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 0.75f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 1f) };

        public override string[] SerieNames { get { return new string[] { "TRAILFIBO.LS", "TRAILFIBO.SS" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            int period = (int)this.parameters[0];
            float upRatio = (float)this.parameters[1];

            var indicator = stockSerie.GetIndicator("FIBOCHANNEL(" + period + "," + upRatio + ")");
            var fiboUp = indicator.Series[1];
            var fiboDown = indicator.Series[2];

            bool upTrend = true;
            for(int i=0; i< period; i++)
            {
                longStopSerie[i] = lowSerie[i];
                shortStopSerie[i] = float.NaN;
            }
            for (int i = period; i < stockSerie.Count; i++)
            {
                if(upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1])
                    { // Trailing stop has been broken => reverse trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = fiboUp[i];
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = Math.Max(longStopSerie[i - 1], fiboDown[i]);
                        shortStopSerie[i] = float.NaN;
                    }
                }
                else
                {
                    if (closeSerie[i] > shortStopSerie[i - 1])
                    {  // Trailing stop has been broken => reverse trend
                        upTrend = true;
                        longStopSerie[i] = fiboDown[i];
                        shortStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = Math.Min(shortStopSerie[i - 1], fiboUp[i]);
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