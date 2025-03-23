using StockAnalyzer.StockMath;
using System;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILFIBO2 : StockTrailStopBase
    {
        public override string[] ParameterNames => new string[] { "HighPeriod", "LowPeriod", "Ratio" };

        public override Object[] ParameterDefaultValues => new Object[] { 75, 75, 0.61f };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 5f) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);

            var highPeriod = (int)this.parameters[0];
            var lowPeriod = (int)this.parameters[1];
            var fiboRatio = (float)this.parameters[2];
            int startPeriod = Math.Max(highPeriod, lowPeriod);

            if (stockSerie.ValueArray.Length < startPeriod)
            {
                // Generate events
                this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
                return;
            }

            var fiboIndicator = stockSerie.GetIndicator(this.Name.Replace("TRAILFIBO2", "FIBOCHANNEL"));

            var fiboHighSerie = fiboIndicator.GetSerie("HIGH");
            var fiboLongSerie = fiboIndicator.GetSerie("FiboUp");
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            bool upTrend = false;

            for (int i = startPeriod; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1])
                    { // Trailing stop has been broken => end up trend
                        upTrend = false;
                        longStopSerie[i] = float.NaN;
                    }
                    else
                    {
                        // Trail the stop  
                        longStopSerie[i] = fiboLongSerie[i];
                    }
                }
                else
                {
                    if (closeSerie[i] > fiboHighSerie[i - 1] && stockSerie.ValueArray[i].VARIATION > 0.05f)
                    {  // new high => up trend starts
                        upTrend = true;
                        longStopSerie[i] = fiboLongSerie[i];
                    }
                }
            }

            this.Series[0] = longStopSerie;
            this.Series[0].Name = longStopSerie.Name;
            this.Series[1] = shortStopSerie;
            this.Series[1].Name = shortStopSerie.Name;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}