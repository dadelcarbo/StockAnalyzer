using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILEMABODY : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop based on first body above the or below the ATR Band.";

        public override string[] ParameterNames => new string[] { "Period", "UseBody", "AtrEntryWidth", "AtrExitWidth" };

        public override Object[] ParameterDefaultValues => new Object[] { 30, true, 0.5f, -0.5f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeBool(), new ParamRangeFloat(-10f, 10f), new ParamRangeFloat(-10f, 10f) };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, SerieNames[1], float.NaN);

            int period = (int)this.parameters[0];
            bool useBody = (bool)this.parameters[1];
            float atrEntryWidth = (float)this.parameters[2];
            float atrExitWidth = (float)this.parameters[3];

            var ema = stockSerie.GetIndicator($"EMA({period})").Series[0];
            var atr = stockSerie.GetIndicator($"ATR({period})").Series[0];
            var longEntryTrigger = ema + atrEntryWidth * atr;
            var longExitTrigger = ema + atrExitWidth * atr;

            var shortEntryTrigger = ema - atrEntryWidth * atr;
            var shortExitTrigger = ema - atrExitWidth * atr;

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var bodyLowSerie = stockSerie.GetSerie(useBody ? StockDataType.BODYLOW : StockDataType.LOW);
            var bodyHighSerie = stockSerie.GetSerie(useBody ? StockDataType.BODYHIGH : StockDataType.HIGH);

            bool upTrend = false;
            bool downTrend = false;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (upTrend)
                {
                    if (closeSerie[i] < longExitTrigger[i])
                    {
                        upTrend = false;
                    }
                    else
                    {
                        longStopSerie[i] = longExitTrigger[i];
                    }
                }
                else
                {
                    if (bodyLowSerie[i] > longEntryTrigger[i])
                    {
                        longStopSerie[i] = longExitTrigger[i];
                        upTrend = true;
                    }
                }

                if (downTrend)
                {
                    if (closeSerie[i] > shortExitTrigger[i])
                    {
                        downTrend = false;
                    }
                    else
                    {
                        shortStopSerie[i] = shortExitTrigger[i];
                    }
                }
                else
                {
                    if (bodyHighSerie[i] < ema[i])
                    {
                        shortStopSerie[i] = shortExitTrigger[i];
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
