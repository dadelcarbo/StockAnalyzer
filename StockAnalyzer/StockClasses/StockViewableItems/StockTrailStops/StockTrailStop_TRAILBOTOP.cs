using System;
using System.Collections.Generic;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILBOTOP : StockTrailStopBase
    {
        public override string Definition => "TRAILBOTOP(int weight)";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override bool RequiresVolumeData => false;

        public override string[] ParameterNames => new string[] { "Weight" };

        public override Object[] ParameterDefaultValues => new Object[] { 1 };

        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "TRAILBOTOP.LS", "TRAILBOTOP.SS" };

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
            float weight = (int)this.parameters[0];
            FloatSerie longStopSerie = new FloatSerie(stockSerie.Count);
            FloatSerie shortStopSerie = new FloatSerie(stockSerie.Count);

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            SortedSet<float> bottoms = new SortedSet<float>();
            SortedSet<float> tops = new SortedSet<float>();

            float stop = Math.Min(lowSerie[0], lowSerie[1]);
            float highest = Math.Max(highSerie[0], highSerie[1]);
            float lowest = stop;
            longStopSerie[0] = longStopSerie[1] = stop;
            shortStopSerie[0] = shortStopSerie[1] = float.NaN;
            bool isLong = true;

            for (int i = 2; i < stockSerie.Count; i++)
            {
                if (isLong)
                {
                    if (closeSerie[i] < stop) // Broken down
                    {
                        isLong = false;
                        stop = highest;
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = stop;
                        lowest = lowSerie[i];
                    }
                    else
                    {
                        highest = Math.Max(highest, highSerie[i]);

                        // Check if need to trail
                        float bottom = lowSerie[i - 1];
                        if (lowSerie[i] > bottom && bottom < lowSerie[i - 2])
                        {
                            stop = (weight * stop + bottom) / (weight + 1f);
                        }
                        longStopSerie[i] = stop;
                        shortStopSerie[i] = float.NaN;
                    }
                }
                else
                {
                    if (closeSerie[i] > stop) // Broken up
                    {
                        isLong = true;
                        stop = lowest;
                        longStopSerie[i] = stop;
                        shortStopSerie[i] = float.NaN;
                        highest = highSerie[i];
                    }
                    else
                    {
                        lowest = Math.Min(lowest, lowSerie[i]);

                        // Check if need to trail
                        float top = lowSerie[i - 1];
                        if (lowSerie[i] < top && top > lowSerie[i - 2])
                        {
                            stop = (weight * stop + top) / (weight + 1f);
                        }
                        longStopSerie[i] = float.NaN;
                        shortStopSerie[i] = stop;
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