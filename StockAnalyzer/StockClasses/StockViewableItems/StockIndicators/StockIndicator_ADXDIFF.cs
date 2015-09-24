using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_ADXDIFF : StockIndicatorBase
   {
      public StockIndicator_ADXDIFF()
      {
      }

      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override string Definition
      {
         get { return "ADXDIFF(int Period, float Smoothing)"; }
      }
      public override object[] ParameterDefaultValues
      {
         get { return new Object[] { 14, 25f }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Smoothing" }; }
      }


      public override string[] SerieNames { get { return new string[] { "ADXDIFF(" + this.Parameters[0].ToString() + ")" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Black) };
            }
            return seriePens;
         }
      }
      static HLine[] lines = null;
      public override HLine[] HorizontalLines
      {
         get
         {
            if (lines == null)
            {
               lines = new HLine[] { new HLine(0, new Pen(Color.DarkGray)) };
               lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            }
            return lines;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         // Set HLine value
         float trendThreshold = 25; // Used only ofr event calculation
         int smoothing = (int)this.Parameters[1];

         int period = (int)this.Parameters[0];
         var adx = stockSerie.GetIndicator("ADX(" + period + "," + trendThreshold + "," + smoothing + ")");

         this.Series[0] = (adx.Series[1] - adx.Series[2]);
         this.Series[0].Name = this.Name;
      }

      static string[] eventNames = new string[] { };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
