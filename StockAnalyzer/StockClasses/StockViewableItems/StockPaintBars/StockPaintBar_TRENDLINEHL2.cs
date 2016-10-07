using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
   public class StockPaintBar_TRENDLINEHL2 : StockPaintBarBase
   {
      public StockPaintBar_TRENDLINEHL2()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }
      public override bool RequiresVolumeData { get { return false; } }

      public override bool HasTrendLine { get { return true; } }

      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "NbPivots" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 1, 10 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 200), new ParamRangeInt(1, 100) }; }
      }

      static string[] eventNames = null;
      public override string[] EventNames
      {
         get
         {
            if (eventNames == null)
            {
               eventNames = Enum.GetNames(typeof(StockSerie.TLEvent));
            }
            return eventNames;
         }
      }
      static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
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

         stockSerie.generateAutomaticHL2TrendLines(0, stockSerie.Count - 1,
             (int)this.parameters[0],
             (int)this.parameters[1],
             ref this.eventSeries);
      }
   }
}
