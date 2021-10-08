using System;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILEMATF : StockTrailStopBase
    {
        public override string Definition => base.Definition + Environment.NewLine +
            "Draw a trail stop that start when a bar is fully above a EMA, and trail stop when the close goes below the EMA";
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 20 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "TRAILEMATF.LS", "TRAILEMATF.SS" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count, "TRAILEMATF.LS", float.NaN);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count, "TRAILEMATF.SS", float.NaN);

            int period = (int)this.parameters[0];

            if (stockSerie.ValueArray.Length < period)
            {
                // Generate events
                this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
                return;
            }

            //var highSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Max(v.OPEN, v.CLOSE)).ToArray());
            //var lowSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)).ToArray());
            var lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            lowSerie = new FloatSerie(stockSerie.Values.Select(v => Math.Min(v.OPEN, v.CLOSE)).ToArray());

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

            FloatSerie emaSerie = stockSerie.GetIndicator($"EMA({period})").Series[0];
            FloatSerie atrSerie = stockSerie.GetIndicator($"ATR({10})").Series[0];

            float trail = float.NaN;
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (float.IsNaN(trail))
                {
                    if (lowSerie[i] > emaSerie[i] && closeSerie[i] > openSerie[i])
                    {
                        trail = emaSerie[i] - atrSerie[i];
                    }
                }
                else
                {
                    if (closeSerie[i] < trail)
                    {
                        trail = float.NaN;
                    }
                    else
                    {
                        trail = Math.Max(emaSerie[i] - atrSerie[i], trail);
                    }
                    //if (lowSerie[i] < emaSerie[i])
                    //{
                    //    trail = Math.Max(lowSerie[i] - atrSerie[i], trail);
                    //}
                }
                longStopSerie[i] = trail;
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