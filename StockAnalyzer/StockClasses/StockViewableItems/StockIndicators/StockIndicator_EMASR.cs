using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EMASR : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SupportResistance;

        public override string Definition => "EMASR(int Period)";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 35 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "EMA.S", "EMA.R" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.Parameters[0];

            FloatSerie supportSerie = new FloatSerie(stockSerie.Count, "EMA.S", float.NaN);
            FloatSerie resistanceSerie = new FloatSerie(stockSerie.Count, "EMA.R", float.NaN);

            this.Series[0] = supportSerie;
            this.Series[1] = resistanceSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            var emaSerie = stockSerie.GetIndicator("EMA(" + period + ")").Series[0];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);

            bool aboveEmaPrevious = closeSerie[1] > emaSerie[1];

            float futureResistance = aboveEmaPrevious ? highSerie.GetMax(0, 1) : float.MinValue;
            float futureSupport = aboveEmaPrevious ? float.MaxValue : lowSerie.GetMin(0, 1);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                bool aboveEma = closeSerie[i] > emaSerie[i];
                if (aboveEma)
                {
                    if (aboveEmaPrevious)
                    {
                        futureResistance = Math.Max(highSerie[i], futureResistance);

                        if (!float.IsNaN(resistanceSerie[i - 1]))
                        {
                            // Check if resistance broken
                            var resistanceBroken = closeSerie[i] > resistanceSerie[i - 1];
                            this.Events[6][i] = resistanceBroken;
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
                            var supportBroken = closeSerie[i] < supportSerie[i - 1];
                            this.Events[6][i] = supportBroken;
                            supportSerie[i] = supportBroken ? float.NaN : supportSerie[i - 1];
                        }
                        resistanceSerie[i] = resistanceSerie[i - 1];
                    }
                }
                aboveEmaPrevious = aboveEma;
            }
        }

        static readonly string[] eventNames = new string[] { "SupportDetected", "ResistanceDetected", "Pullback", "EndOfTrend", "HigherLow", "LowerHigh", "ResistanceBroken", "SupportBroken", "Bullish", "Bearish", "HigherHigh", "LowerLow" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, true, true, false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
