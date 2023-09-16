using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILNUP : StockTrailStopBase
    {

        public override string Definition => "Draw Trail Stop starting after n consecutive up days";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 2 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;
            int period = (int)this.Parameters[0];

            longStopSerie = new FloatSerie(stockSerie.Count, "TRAILNUP.LS", float.NaN);
            shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILNUP.SS", float.NaN);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.BODYLOW);

            bool upTrend = false;
            int upCount = 0;
            for (int i = period; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longStopSerie[i - 1])
                    {
                        upTrend = false;
                    }
                    else
                    {
                        longStopSerie[i] = lowSerie.GetMin(i - period, i);
                    }
                }
                else
                {
                    if (closeSerie[i] > closeSerie[i - 1])
                    {
                        if (++upCount >= period)
                        {
                            upTrend = true;
                            longStopSerie[i] = lowSerie.GetMin(i - period, i);
                            upCount = 0;
                        }
                    }
                    else
                    {
                        upCount = 0;
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