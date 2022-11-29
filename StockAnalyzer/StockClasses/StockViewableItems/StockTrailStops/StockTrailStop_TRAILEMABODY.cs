using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILEMABODY : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop based on first body above the or below the EMA.";

        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "UseBody" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 30, true }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeBool() }; }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);

            int period = (int)this.parameters[0];
            bool useBody = (bool)this.parameters[1];

            var ema = stockSerie.GetIndicator($"EMA({period})").Series[0];
            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var bodyLowSerie = stockSerie.GetSerie(useBody ? StockDataType.BODYLOW : StockDataType.LOW);
            var bodyHighSerie = stockSerie.GetSerie(useBody ? StockDataType.BODYHIGH : StockDataType.HIGH);

            bool upTrend = false;
            bool downTrend = false;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < ema[i])
                    {
                        upTrend = false;
                    }
                    else
                    {
                        longStopSerie[i] = ema[i];
                    }
                }
                else
                {
                    if (bodyLowSerie[i] > ema[i])
                    {
                        longStopSerie[i] = ema[i];
                        upTrend = true;
                    }
                }

                if (downTrend)
                {
                    if (closeSerie[i] > ema[i])
                    {
                        downTrend = false;
                    }
                    else
                    {
                        shortStopSerie[i] = ema[i];
                    }
                }
                else
                {
                    if (bodyHighSerie[i] < ema[i])
                    {
                        shortStopSerie[i] = ema[i];
                        downTrend = true;
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
