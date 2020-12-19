using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_SAR : StockIndicatorBase
   {
      public StockIndicator_SAR()
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


      public override string[] ParameterNames
      {
         get { return new string[] { "Init", "Step", "Max", "InputSmooting" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 0.02f, 0.02f, 0.2f, 1 }; }
      }

      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeFloat(0.0f, 10.0f), new ParamRangeFloat(0.0f, 10.0f), new ParamRangeFloat(0.01f, 100.0f), new ParamRangeInt(1,500)}; }
      }

      public override string[] SerieNames
      {
         get { return new string[] { "SAR.S", "SAR.R" }; }
      }

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
         float accelerationFactorStart = (float)this.parameters[0];
         float accelerationFactorStep = (float)this.parameters[1];
         float accelerationFactorMax = (float)this.parameters[2];

         FloatSerie sarSupport;
         FloatSerie sarResistance;

         stockSerie.CalculateSAR(accelerationFactorStep, accelerationFactorStart, accelerationFactorMax, out sarSupport,
            out sarResistance, (int)this.parameters[3]);

         this.Series[0] = sarSupport;
         this.Series[1] = sarResistance;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);
         float previousHigh = stockSerie.Values.First().HIGH, previousLow = stockSerie.Values.First().LOW;
         float previousHigh2 = stockSerie.Values.First().HIGH, previousLow2 = stockSerie.Values.First().LOW;
         bool waitingForEndTrend = false;
         for (int i = 5; i < stockSerie.Count; i++)
         {
            if (float.IsNaN(sarResistance[i]) && !float.IsNaN(sarResistance[i - 1]))
            {
               this.Events[0][i] = true; // SupportDetected

               if (waitingForEndTrend)
               {
                  this.Events[3][i] = true; // EndOfTrend
                  waitingForEndTrend = false;
               }

               if (sarSupport[i] > previousLow)
               {
                  this.Events[4][i] = true; // HigherLow

                  if (sarSupport[i] > previousHigh2)
                  {
                     this.Events[2][i] = true; // PB
                     waitingForEndTrend = true;
                  }
               }
               previousLow2 = previousLow;
               previousLow = sarSupport[i];
            }
            if (float.IsNaN(sarSupport[i]) && !float.IsNaN(sarSupport[i - 1]))
            {
               this.Events[1][i] = true; // ResistanceDetected

               if (waitingForEndTrend)
               {
                  this.Events[3][i] = true; // EndOfTrend
                  waitingForEndTrend = false;
               }

               if (sarResistance[i] < previousHigh)
               {
                  this.Events[5][i] = true; // LowerHigh
                  if (sarResistance[i] < previousLow2)
                  {
                     this.Events[2][i] = true; // PB
                     waitingForEndTrend = true;
                  }
               }
               previousHigh2 = previousHigh;
               previousHigh = sarResistance[i];
            }

            this.Events[6][i] = false;
            this.Events[7][i] = false;
            this.Events[8][i] = float.IsNaN(sarResistance[i]);
            this.Events[9][i] = float.IsNaN(sarSupport[i]);
         }
      }

      private static string[] eventNames = new string[]
      {
         "SupportDetected", "ResistanceDetected", "Pullback", "EndOfTrend", "HigherLow", "LowerHigh", "ResistanceBroken",
         "SupportBroken", "Bullish", "Bearish"
      };

      //static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BrokenUp", "BrokenDown" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }

      private static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, true, true, false, false };
      //static readonly bool[] isEvent = new bool[] { false, false, true, true };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}