using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_TRAILHLSR : StockUpDownIndicatorBase
   {
      public StockIndicator_TRAILHLSR()
      {
      }
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
         get { return "TRAILHLSR(int Period)"; }
      }

      public override string[] ParameterNames
      {
         get { return new string[] { "Period" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 1 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(0, 50) }; }
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
         Queue<float> resistanceQueue = new Queue<float>(new float[] { float.MinValue, float.MinValue });
         Queue<float> supportQueue = new Queue<float>(new float[] { float.MaxValue, float.MaxValue });

         FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
         FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         int period = (int)this.Parameters[0];

         IStockTrailStop trailStop = stockSerie.GetTrailStop("TRAILHL(" + period + ")");

         FloatSerie longStopSerie = trailStop.Series[0];
         FloatSerie shortStopSerie = trailStop.Series[1];

         BoolSerie brokenUpSerie = trailStop.Events[2];
         BoolSerie brokenDownSerie = trailStop.Events[3];

         FloatSerie supportSerie = new FloatSerie(stockSerie.Count, "TRAILHL.S"); supportSerie.Reset(float.NaN);
         FloatSerie resistanceSerie = new FloatSerie(stockSerie.Count, "TRAILHL.R"); resistanceSerie.Reset(float.NaN);


         this.Series[0] = supportSerie;
         this.Series[1] = resistanceSerie;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);
         this.Events[0] = brokenUpSerie;
         this.Events[1] = brokenDownSerie;

         // Begin Sequence

         // Calculate Support/Resistance
         float extremum = lowSerie[0];
         bool waitingForEndOfTrend = false;
         int i = 0;
         for (; i < stockSerie.Count && (!brokenUpSerie[i] && !brokenDownSerie[i]); i++)
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
            if (brokenUpSerie[i])
            {
               this.UpDownState[i] = StockSerie.Trend.UpTrend;
               extremum = lowSerie.GetMin(0, i);
            }
            if (brokenDownSerie[i])
            {
               this.UpDownState[i] = StockSerie.Trend.DownTrend;
               extremum = highSerie.GetMax(0, i);
            }
         }

         for (; i < stockSerie.Count; i++)
         {
            bool upSwing = float.IsNaN(shortStopSerie[i]);
            this.UpDownState[i] = StockUpDownIndicatorBase.BoolToTrend(upSwing);

            this.Events[8][i] = upSwing;
            this.Events[9][i] = !upSwing;

            if (brokenUpSerie[i])
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

               extremum = highSerie[i];
            }
            else if (brokenDownSerie[i])
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

      static string[] eventNames = new string[] { "SupportDetected", "ResistanceDetected", "Pullback", "EndOfTrend", "HigherLow", "LowerHigh", "ResistanceBroken", "SupportBroken", "Bullish", "Bearish" };
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
