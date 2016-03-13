using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
   public class StockPaintBar_COTCOMMERCIAL : StockPaintBarBase
   {
      public StockPaintBar_COTCOMMERCIAL()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }

      public override string[] ParameterNames
      {
         get { return new string[] { "Period" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 20 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 200) }; }
      }

      static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BrokenUp", "BrokenDown", "TrailedUp", "TrailedDown" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }

      static readonly bool[] isEvent = new bool[] { false, false, true, true, true, true };
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
         if (stockSerie.CotSerie != null)
         {
            IStockTrail stockTrail = stockSerie.GetTrail("HL("+(int)this.parameters[0]+")", "COTCOMMERCIAL(1,False,True)");
            this.eventSeries = stockTrail.Events;
         }
         else
         {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
         }
      }
   }
}
