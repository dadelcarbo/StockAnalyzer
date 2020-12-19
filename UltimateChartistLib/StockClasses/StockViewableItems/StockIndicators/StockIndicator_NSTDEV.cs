using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_NSTDEV : StockIndicatorBase
   {
      public StockIndicator_NSTDEV()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override string Name
      {
         get { return "NSTDEV(" + this.Parameters[0].ToString() + ")"; }
      }

      public override string Definition
      {
         get { return "NSTDEV(int Period)"; }
      }
      public override object[] ParameterDefaultValues
      {
         get { return new Object[] { 20 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period" }; }
      }

      public override string[] SerieNames { get { return new string[] { "NSTDEV(" + this.Parameters[0].ToString() + ")" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Blue) };
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
               lines = new HLine[] { new HLine(0, new Pen(Color.Gray)) };
               lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            }
            return lines;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         int period = (int)this.parameters[0];
         FloatSerie stdevSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateStdev(period);
         FloatSerie emaSerie = stockSerie.GetIndicator("EMA(" + period + ")").Series[0];
         FloatSerie stdevCount = new FloatSerie(emaSerie.Count, this.SerieNames[0]);
         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         for (int i = period; i < emaSerie.Count; i++)
         {
            if (stdevSerie[i] >= -0.00001f && stdevSerie[i] <= 0.00001f)
            {
               stdevCount[i] = 0;
            }
            else
            {
               stdevCount[i] = (closeSerie[i] - emaSerie[i]) / stdevSerie[i];
            }
         }
         this.Series[0] = stdevCount;
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
