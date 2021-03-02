using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILGUPPY : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that is calculated with the guppy ema bands.";

        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }

        public override string[] ParameterNames => new string[] { "FastPeriod1", "SlowPeriod1", "FastPeriod2", "SlowPeriod2" };
        public override Object[] ParameterDefaultValues => new Object[] { 3, 15, 30, 60 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000), new ParamRangeInt(1, 1000) };

        public override string[] SerieNames { get { return new string[] { "TRAILGUPPY.LS", "TRAILGUPPY.SS" }; } }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[0], float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, this.SerieNames[1], float.NaN);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            int fastPeriod1 = (int)this.parameters[0];
            int slowPeriod1 = (int)this.parameters[1];
            int fastPeriod2 = (int)this.parameters[2];
            int slowPeriod2 = (int)this.parameters[3];

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            var fastEMA1 = closeSerie.CalculateEMA(fastPeriod1);
            var slowEMA1 = closeSerie.CalculateEMA(slowPeriod1);
            var fastEMA2 = closeSerie.CalculateEMA(fastPeriod2);
            var slowEMA2 = closeSerie.CalculateEMA(slowPeriod2);

            var osc1 = fastEMA1 - slowEMA1;
            var osc2 = fastEMA2 - slowEMA2;

            bool bullish = false;
            bool bearish = false;
            for (int i = slowPeriod2; i < stockSerie.Count; i++)
            {
                if (bullish)
                {
                    if (closeSerie[i] < longStopSerie[i - 1])
                    {
                        bullish = false;
                    }
                    else
                    {
                        longStopSerie[i] = slowEMA2[i];
                    }
                }
                else
                {
                    if (osc2[i] > 0 && osc2[i - 1] < 0)
                    {
                        bullish = true;
                        longStopSerie[i] = slowEMA2[i];
                    }
                }
                if (bearish)
                {
                    if (closeSerie[i] > shortStopSerie[i - 1])
                    {
                        bearish = false;
                    }
                    else
                    {
                        shortStopSerie[i] = slowEMA2[i];
                    }
                }
                else
                {
                    if (osc2[i] < 0 && osc1[i - 1] > 0 && osc1[i] < 0)
                    {
                        bearish = true;
                        shortStopSerie[i] = slowEMA2[i];
                    }
                }
            }

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
    }
}
