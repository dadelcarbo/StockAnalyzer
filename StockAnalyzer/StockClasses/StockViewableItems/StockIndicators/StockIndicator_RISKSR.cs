using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_RISKSR : StockUpDownIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override IndicatorDisplayStyle DisplayStyle
        {
            get { return IndicatorDisplayStyle.SupportResistance; }
        }

        public override string Definition
        {
            get { return "TRAILHIGHERLOWSR(int Period)"; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Period" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 80 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(0, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { "TRAILHL.S", "TRAILHL.R" }; } }

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
            int period = (int)this.Parameters[0];

            IStockIndicator trailStop = stockSerie.GetIndicator("TOPEMA(0," + period + ",1)");

            FloatSerie longStopSerie = trailStop.Series[0];
            FloatSerie shortStopSerie = trailStop.Series[1];

            // "SupportDetected", "ResistanceDetected", // 0,1

            BoolSerie supportDetectedSerie = trailStop.Events[0];
            BoolSerie resistanceDetectedSerie = trailStop.Events[1];

            FloatSerie supportSerie = new FloatSerie(stockSerie.Count, "TRAILHL.S"); supportSerie.Reset(float.NaN);
            FloatSerie resistanceSerie = new FloatSerie(stockSerie.Count, "TRAILHL.R"); resistanceSerie.Reset(float.NaN);

            this.Series[0] = supportSerie;
            this.Series[1] = resistanceSerie;

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            Stack<float> supportStack = new Stack<float>();
            Stack<float> resistanceStack = new Stack<float>();

            // Calculate long stop
            bool bullish = false;
            bool bearish = false;
            int i = 0;
            for (i = 1; i < stockSerie.Count; i++)
            {
                if (supportDetectedSerie[i])
                {
                    while (supportStack.Count > 0 && longStopSerie[i] < supportStack.Peek())
                    {
                        supportStack.Pop();
                    }
                    supportStack.Push(longStopSerie[i]);

                    if (supportStack.Count > 1)
                    {
                        supportSerie[i] = supportStack.ElementAt(1);
                    }
                }
                else
                {
                    if (!float.IsNaN(supportSerie[i - 1]))
                    {
                        if (closeSerie[i] > supportSerie[i - 1])
                        {
                            supportSerie[i] = supportSerie[i - 1];
                        }
                        else
                        {
                            bullish = false;
                            bearish = true;
                            this.Events[0][i] = true;
                        }
                    }
                }
                if (resistanceDetectedSerie[i])
                {
                    while (resistanceStack.Count > 0 && shortStopSerie[i] > resistanceStack.Peek())
                    {
                        resistanceStack.Pop();
                    }
                    resistanceStack.Push(shortStopSerie[i]);

                    if (resistanceStack.Count > 1)
                    {
                        resistanceSerie[i] = resistanceStack.ElementAt(1);
                    }
                }
                else
                {
                    if (!float.IsNaN(resistanceSerie[i - 1]))
                    {
                        if (closeSerie[i] < resistanceSerie[i - 1])
                        {
                            resistanceSerie[i] = resistanceSerie[i - 1];
                        }
                        else
                        {
                            bullish = true;
                            bearish = false;
                            this.Events[1][i] = true;
                        }
                    }
                }
                this.Events[8][i] = bullish;
                this.Events[9][i] = bearish;
            }
        }

        static string[] eventNames = new string[] {
            "SupportBroken", "ResistanceBroken",        // 0,1
            "Pullback", "EndOfTrend",                   // 2,3
            "HigherLow", "LowerHigh",                   // 4,5
            "ResistanceBroken", "SupportBroken",        // 6,7
            "Bullish", "Bearish"                        // 8,9
             };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
