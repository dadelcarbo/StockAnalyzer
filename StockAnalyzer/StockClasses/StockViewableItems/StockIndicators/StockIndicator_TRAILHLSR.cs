using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TRAILHLSR : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SupportResistance;

        public override string Definition => "TRAILHLSR(int Period)";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 1 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "TRAILHL.S", "TRAILHL.R" };

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

            IStockTrailStop trailStop = stockSerie.GetTrailStop("TRAILHL(" + period + ")");

            FloatSerie longStopSerie = trailStop.Series[0];
            FloatSerie shortStopSerie = trailStop.Series[1];

            BoolSerie supportDetectedSerie = trailStop.Events[0];
            BoolSerie resistanceDetectedSerie = trailStop.Events[1];

            FloatSerie supportSerie = new FloatSerie(stockSerie.Count, "TRAILHL.S"); supportSerie.Reset(float.NaN);
            FloatSerie resistanceSerie = new FloatSerie(stockSerie.Count, "TRAILHL.R"); resistanceSerie.Reset(float.NaN);

            this.Series[0] = supportSerie;
            this.Series[1] = resistanceSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            Queue<float> resistanceQueue = new Queue<float>(new float[] { float.MinValue, float.MinValue });
            Queue<float> supportQueue = new Queue<float>(new float[] { float.MaxValue, float.MaxValue });

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            this.Events[0] = supportDetectedSerie;
            this.Events[1] = resistanceDetectedSerie;

            // Begin Sequence

            // Calculate Support/Resistance
            float extremum = lowSerie[0];
            bool waitingForEndOfTrend = false;
            int i = 0;
            for (; i < stockSerie.Count && (!supportDetectedSerie[i] && !resistanceDetectedSerie[i]); i++)
            {
                //if (float.IsNaN(longStopSerie[i]))
                //{
                //    this.UpDownState[i] = StockSerie.Trend.DownTrend; // Down trend
                //    supportSerie[i] = float.NaN;
                //    resistanceSerie[i] = highSerie.GetMax(0, i); 
                //    resistanceQueue.Dequeue();
                //    resistanceQueue.Enqueue(resistanceSerie[i]);
                //    extremum = highSerie.GetMax(0, i);
                //}
                //else
                //{
                //    this.UpDownState[i] = StockSerie.Trend.UpTrend; // Up trend

                //    supportSerie[i] = lowSerie.GetMin(0, i);
                //    supportQueue.Dequeue();
                //    supportQueue.Enqueue(supportSerie[i]);
                //    resistanceSerie[i] = float.NaN;
                //    extremum = lowSerie.GetMin(0, i);
                //}
            }
            if (i < stockSerie.Count)
            {
                if (supportDetectedSerie[i])
                {
                    extremum = lowSerie.GetMin(0, i);
                }
                if (resistanceDetectedSerie[i])
                {
                    extremum = highSerie.GetMax(0, i);
                }
            }

            for (; i < stockSerie.Count; i++)
            {
                bool upSwing = float.IsNaN(shortStopSerie[i]);

                this.Events[8][i] = upSwing;
                this.Events[9][i] = !upSwing;

                if (supportDetectedSerie[i])
                {
                    supportSerie[i] = extremum;
                    supportQueue.Dequeue();
                    supportQueue.Enqueue(extremum);
                    resistanceSerie[i] = float.NaN;

                    if (waitingForEndOfTrend)
                    {// Detect EndOfUptrend
                        waitingForEndOfTrend = false;
                        this.Events[3][i] = true;
                    }
                    else if (extremum > resistanceQueue.ElementAt(0))
                    {// Detect if pullback in uptrend
                        this.Events[2][i] = true;
                        waitingForEndOfTrend = true;
                    }

                    if (extremum > supportQueue.ElementAt(0))
                    {
                        // Higher Low detected
                        this.Events[4][i] = true;
                    }
                    else
                    {
                        // Lower Low
                        this.Events[11][i] = true;
                    }

                    extremum = highSerie[i];
                }
                else if (resistanceDetectedSerie[i])
                {
                    supportSerie[i] = float.NaN;
                    resistanceSerie[i] = extremum;
                    resistanceQueue.Dequeue();
                    resistanceQueue.Enqueue(extremum);

                    if (waitingForEndOfTrend)
                    {// Detect EndOfUptrend
                        waitingForEndOfTrend = false;
                        this.Events[3][i] = true;
                    }
                    else if (extremum < supportQueue.ElementAt(0))
                    {// Detect if pullback in downTrend
                        this.Events[2][i] = true;
                        waitingForEndOfTrend = true;
                    }

                    if (extremum < resistanceQueue.ElementAt(0))
                    {
                        // Lower high detected
                        this.Events[5][i] = true;
                    }
                    else
                    {
                        // Higher high detected
                        this.Events[10][i] = true;
                    }

                    extremum = lowSerie[i];
                }
                else
                {
                    supportSerie[i] = supportSerie[i - 1];
                    resistanceSerie[i] = resistanceSerie[i - 1];
                    if (float.IsNaN(supportSerie[i])) // Down trend
                    {
                        extremum = Math.Min(extremum, lowSerie[i]);
                        if (closeSerie[i - 1] >= supportQueue.ElementAt(1) && closeSerie[i] < supportQueue.ElementAt(1))
                        {
                            // Previous support broken
                            this.Events[7][i] = true;
                        }
                    }
                    else
                    {
                        extremum = Math.Max(extremum, highSerie[i]);
                        if (closeSerie[i - 1] <= resistanceQueue.ElementAt(1) && closeSerie[i] > resistanceQueue.ElementAt(1))
                        {
                            // Previous resistance broken
                            this.Events[6][i] = true;
                        }
                    }
                }
            }
        }

        static string[] eventNames = new string[] { "SupportDetected", "ResistanceDetected", "Pullback", "EndOfTrend", "HigherLow", "LowerHigh", "ResistanceBroken", "SupportBroken", "Bullish", "Bearish", "HigherHigh", "LowerLow" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, true, true, false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}
