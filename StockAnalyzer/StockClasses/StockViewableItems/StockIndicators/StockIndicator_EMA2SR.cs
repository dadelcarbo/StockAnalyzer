using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EMA2SR : StockSRIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SupportResistance;

        public override string[] ParameterNames => new string[] { "SlowPeriod", "FastPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 35, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500), new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "EMA2.S", "EMA2.R" };

        public override void ApplyTo(StockSerie stockSerie)
        {
            int slowPeriod = (int)this.Parameters[0];
            int fastPeriod = (int)this.Parameters[1];

            FloatSerie supportSerie = new FloatSerie(stockSerie.Count, "EMA.S", float.NaN);
            FloatSerie resistanceSerie = new FloatSerie(stockSerie.Count, "EMA.R", float.NaN);

            this.Series[0] = supportSerie;
            this.Series[1] = resistanceSerie;

            // Detecting events
            var slowEmaSerie = stockSerie.GetIndicator("EMA(" + slowPeriod + ")").Series[0];
            var fastEmaSerie = stockSerie.GetIndicator("EMA(" + fastPeriod + ")").Series[0];

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);

            bool aboveEmaPrevious = fastEmaSerie[1] > slowEmaSerie[1];

            float futureResistance = aboveEmaPrevious ? highSerie.GetMax(0, 1) : float.MinValue;
            float futureSupport = aboveEmaPrevious ? float.MaxValue : lowSerie.GetMin(0, 1);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                bool aboveEma = fastEmaSerie[i] > slowEmaSerie[i];
                if (aboveEma)
                {
                    if (aboveEmaPrevious)
                    {
                        futureResistance = Math.Max(highSerie[i], futureResistance);

                        if (!float.IsNaN(resistanceSerie[i - 1]))
                        {
                            // Check if resistance broken
                            var resistanceBroken = fastEmaSerie[i] > resistanceSerie[i - 1];
                            resistanceSerie[i] = resistanceBroken ? float.NaN : resistanceSerie[i - 1];
                        }
                        supportSerie[i] = supportSerie[i - 1];
                    }
                    else
                    {
                        // Close has crossed above EMA, detect previous support as lowest low below EMA
                        supportSerie[i] = futureSupport;
                        futureSupport = float.MaxValue;
                        futureResistance = highSerie[i];
                        resistanceSerie[i] = resistanceSerie[i - 1];
                    }
                }
                else
                {
                    if (aboveEmaPrevious)
                    {
                        // Close has crossed below EMA, detect previous resistance as highest high above EMA
                        resistanceSerie[i] = futureResistance;
                        futureSupport = lowSerie[i];
                        futureResistance = float.MinValue;
                        supportSerie[i] = supportSerie[i - 1];
                    }
                    else
                    {
                        futureSupport = Math.Min(lowSerie[i], futureSupport);
                        if (!float.IsNaN(supportSerie[i - 1]))
                        {
                            // Check if support broken
                            var supportBroken = fastEmaSerie[i] < supportSerie[i - 1];
                            supportSerie[i] = supportBroken ? float.NaN : supportSerie[i - 1];
                        }
                        resistanceSerie[i] = resistanceSerie[i - 1];
                    }
                }
                aboveEmaPrevious = aboveEma;
            }

            GenerateEvents(stockSerie, supportSerie, resistanceSerie);
        }

    }
}
