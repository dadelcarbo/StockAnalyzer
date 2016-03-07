using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_ATRPERCENT : StockIndicatorBase
   {
      public StockIndicator_ATRPERCENT()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override object[] ParameterDefaultValues
      {
         get { return new Object[] { 20, 1 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "InputSmoothing" }; }
      }

      public override string[] SerieNames { get { return new string[] { "ATRPERCENT(" + this.Parameters[0].ToString() + ")" }; } }

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
      public override HLine[] HorizontalLines
      {
         get { return null; }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH).CalculateEMA((int)this.Parameters[1]);
         FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW).CalculateEMA((int)this.Parameters[1]);
         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA((int)this.Parameters[1]);

         FloatSerie atrSerie = new FloatSerie(stockSerie.Count);

         for (int i = 1; i < stockSerie.Count; i++)
         {
            atrSerie[i] = 100f*(highSerie[i] - lowSerie[i]) / closeSerie[i - 1];
         }

         this.series[0] = atrSerie.CalculateEMA((int)this.Parameters[0]);
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
