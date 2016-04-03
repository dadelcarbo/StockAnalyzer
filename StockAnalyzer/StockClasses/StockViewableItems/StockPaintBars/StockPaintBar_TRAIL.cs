using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrails;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockPaintBars
{
   public class StockPaintBar_TRAIL : StockPaintBarBase
   {
      public StockPaintBar_TRAIL()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }
      public override bool RequiresVolumeData { get { return false; } }
      
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "NbPivots" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { "HL(10)", "ER(20_1_1)" }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeTrail(), new ParamRangeIndicator() }; }
      }

      static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BrokenUp", "BrokenDown", "TrailedUp", "TrailedDown" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false };
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
               seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Red) };
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
         IStockTrail stockTrail = stockSerie.GetTrail(this.parameters[0].ToString().Replace('_', ','), 
            this.parameters[1].ToString().Replace('_', ','));
         
         int i = 0;
         foreach (BoolSerie boolSerie in stockTrail.Events)
         {
            this.Events[i++] = boolSerie;
         }
      }
   }
}
