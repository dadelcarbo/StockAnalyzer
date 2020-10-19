using System;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILTLB : StockTrailStopBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 2 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "TRAILTLB.LS", "TRAILTLB.SS" };

        struct HighLow
        {
            public float high;
            public float low;
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0]);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1]);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var tmpBarList = stockSerie.Take(3).Select(v => new HighLow { high = v.Value.HIGH, low = v.Value.LOW }).ToList();
            longStopSerie[0] = longStopSerie[1] = longStopSerie[2] = tmpBarList.Min(v => v.low);
            shortStopSerie[0] = shortStopSerie[1] = shortStopSerie[2] = float.NaN;
            bool upTrend = true;
            for (int i = 3; i < stockSerie.Count; i++)
            {
                var periodHigh = tmpBarList.Max(v => v.high);
                var periodLow = tmpBarList.Min(v => v.low);
                var close = closeSerie[i];
                if (upTrend)
                {
                    if (close > periodHigh) // New Up bar
                    {
                        tmpBarList.RemoveAt(0); tmpBarList.Add(new HighLow { high = highSerie[i], low = lowSerie[i] });
                        longStopSerie[i] = periodLow;
                        shortStopSerie[i] = float.NaN;
                    }
                    else if (close < periodLow) // reverse bar
                    {
                        tmpBarList.RemoveAt(0); tmpBarList.Add(new HighLow { high = highSerie[i], low = lowSerie[i] });
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = periodHigh;
                        upTrend = false;
                    }
                    else // Same bar
                    {
                        longStopSerie[i] = periodLow;
                        shortStopSerie[i] = float.NaN;
                    }
                }
                else
                {
                    if (close < periodLow) // New Low bar
                    {
                        tmpBarList.RemoveAt(0); tmpBarList.Add(new HighLow { high = highSerie[i], low = lowSerie[i] });
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = periodHigh;
                    }
                    else if (close > periodHigh) // reverse bar
                    {
                        tmpBarList.RemoveAt(0); tmpBarList.Add(new HighLow { high = highSerie[i], low = lowSerie[i] });
                        longStopSerie[i] = periodLow;
                        shortStopSerie[i] = float.NaN;
                        upTrend = true;
                    }
                    else // Same bar
                    {
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = periodHigh;
                    }
                }
            }

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}