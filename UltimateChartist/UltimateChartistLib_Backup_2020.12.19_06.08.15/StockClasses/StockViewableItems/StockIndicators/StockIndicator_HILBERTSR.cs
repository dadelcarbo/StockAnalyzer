using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_HILBERTSR : StockIndicatorBase
   {
      public StockIndicator_HILBERTSR()
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

      public override string Name
      {
         get { return "HILBERTSR(" + this.Parameters[0] + "," + this.Parameters[1] + ")"; }
      }
      public override string Definition
      {
         get { return "HILBERTSR(int Smoothing, int WaitPeriod)"; }
      }

      public override string[] ParameterNames
      {
         get { return new string[] { "Smoothing", "WaitPeriod" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 1, 10 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(0, 500) }; }
      }

      public override string[] SerieNames { get { return new string[] { "HILBERT.S", "HILBERT.R", "SECONDARY.S", "SECONDARY.R" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2), new Pen(Color.Gray, 2), new Pen(Color.Gray, 2) };
            }
            return seriePens;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         FloatSerie hilbertSupport, hilbertResistance, secondarySupport, secondaryResistance;
         IStockIndicator hilbertIndicator = stockSerie.GetIndicator("HILBERT(" + (int)this.Parameters[0] + ")");
         stockSerie.CalculateHilbertSR(hilbertIndicator, (int)this.Parameters[1], out hilbertSupport, out hilbertResistance, out secondarySupport, out secondaryResistance);
         this.Series[0] = hilbertSupport;
         this.Series[1] = hilbertResistance;
         this.Series[2] = secondarySupport;
         this.Series[3] = secondaryResistance;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         float lastSupportLevel = hilbertResistance[0];
         float lastResistanceLevel = hilbertResistance[0];
         bool waitingEndOfUpTrend = false;
         bool waitingEndOfDownTrend = false;
         bool supportDetected = false;
         bool resistanceDetected = false;
         for (int i = 10; i < stockSerie.Count; i++)
         {
            if (float.IsNaN(hilbertResistance[i]))
            {
               // Set UpSwing
               this.eventSeries[4][i] = closeSerie[i] > hilbertSupport[i];
               // Set DownTrend
               this.eventSeries[7][i] = closeSerie[i] < hilbertSupport[i];
            }
            else
            {
               // Set DownSwing
               this.eventSeries[5][i] = closeSerie[i] < hilbertResistance[i];
               // Set UpTrend
               this.eventSeries[6][i] = closeSerie[i] > hilbertResistance[i];
            }

            supportDetected = float.IsNaN(hilbertSupport[i - 1]) && !float.IsNaN(hilbertSupport[i]);
            this.eventSeries[0][i] = supportDetected;

            if (supportDetected)
            {
               if (waitingEndOfDownTrend && hilbertSupport[i] <= lastResistanceLevel) // End of down trend
               {
                  this.eventSeries[3][i] = true;
                  waitingEndOfDownTrend = false; // In any case we don't wait for the EOT as either it's reached or the trend changed.
               }
               else
               {
                  if (hilbertSupport[i] >= lastResistanceLevel) // Pullback in up trend
                  {
                     this.eventSeries[2][i] = true;
                     waitingEndOfUpTrend = true;
                     waitingEndOfDownTrend = false;
                  }
                  else
                  {
                     waitingEndOfUpTrend = false;
                  }
               }
               lastSupportLevel = hilbertSupport[i];
               continue;
            }

            resistanceDetected = float.IsNaN(hilbertResistance[i - 1]) && !float.IsNaN(hilbertResistance[i]);
            this.eventSeries[1][i] = resistanceDetected;

            if (resistanceDetected)
            {
               if (waitingEndOfUpTrend && lastSupportLevel <= hilbertResistance[i]) // End of up trend
               {
                  this.eventSeries[3][i] = true;
                  waitingEndOfUpTrend = false; // In any case we don't wait for the EOT as either it's reached or the trend changed.
               }
               else
               {
                  if (lastSupportLevel >= hilbertResistance[i]) // Pullback in down trend
                  {
                     this.eventSeries[2][i] = true;
                     waitingEndOfUpTrend = false;
                     waitingEndOfDownTrend = true;
                  }
                  else
                  {
                     waitingEndOfUpTrend = false;
                     waitingEndOfUpTrend = false;
                  }
               }
               lastResistanceLevel = hilbertResistance[i];
            }
         }
      }

      static string[] eventNames = new string[] { "SupportDetected", "ResistanceDetected", "Pullback", "EndOfTrend", "UpSwing", "DownSwing", "UpTrend", "DownTrend" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false, false, false };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
