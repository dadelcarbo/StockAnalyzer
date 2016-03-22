using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockDrawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
   public class StockPaintBar_DRAWING : StockPaintBarBase
   {
      public StockPaintBar_DRAWING()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }
      public override bool RequiresVolumeData { get { return false; } }
      
      public override string[] ParameterNames
      {
         get { return new string[] { "EMAPeriod" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 1 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500),  }; }
      }

      static string[] eventNames = null;
      public override string[] EventNames
      {
         get
         {
            if (eventNames == null)
            {
               eventNames = new string[] {"SupportBroken", "ResistanceBroken"};
            }
            return eventNames;
         }
      }

      static readonly bool[] isEvent = new bool[] { true, true };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Green), new Pen(Color.Red) };
               foreach (Pen pen in seriePens)
               {
                  pen.Width = 2;
               }
            }
            return seriePens;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA((int)this.parameters[0]);

         if (stockSerie.StockAnalysis.DrawingItems.ContainsKey(stockSerie.BarDuration))
         {
            var drawingItems = stockSerie.StockAnalysis.DrawingItems[stockSerie.BarDuration].Where(di => di is Line2DBase);
            for (int i = 20; i < stockSerie.Count; i++)
            {
               foreach (Line2DBase item in drawingItems)
               {
                  if (item.ContainsAbsciss(i) && item.ContainsAbsciss(i - 1))
                  {
                     float itemValue = item.ValueAtX(i);
                     float itemPreviousValue = item.ValueAtX(i - 1);
                     if (closeSerie[i - 1] < itemPreviousValue && closeSerie[i] > itemPreviousValue)
                     {
                        // Resistance Broken
                        this.Events[1][i] = true;
                     }
                     else if (closeSerie[i - 1] > itemPreviousValue && closeSerie[i] < itemPreviousValue)
                     {
                        // Support Broken
                        this.Events[0][i] = true;
                     }
                  }
               }
            }
         }
      }
   }
}
