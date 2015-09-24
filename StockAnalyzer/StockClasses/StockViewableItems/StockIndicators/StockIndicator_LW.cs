using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_LW : StockIndicatorBase
   {
      public StockIndicator_LW()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override string Name
      {
         get { return "LW(" + this.Parameters[0].ToString() + ")"; }
      }

      public override string Definition
      {
         get { return "LW(int Period)"; }
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

      public override string[] SerieNames { get { return new string[] { "LW(" + this.Parameters[0].ToString() + ")" }; } }

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
         FloatSerie atrSerie = stockSerie.GetSerie(StockDataType.ATR).CalculateEMA((int)this.Parameters[0]);
         FloatSerie lowCloseSerie = new FloatSerie(stockSerie.Count);
         int i = 0;
         foreach (StockDailyValue dailyValue in stockSerie.Values)
         {
            lowCloseSerie[i++] = dailyValue.CLOSE - dailyValue.LOW;
         }
         lowCloseSerie = lowCloseSerie.CalculateEMA((int)this.Parameters[0]);
         lowCloseSerie = lowCloseSerie / atrSerie;
         lowCloseSerie = (lowCloseSerie * 100.0f) - 50.0f;

         this.series[0] = lowCloseSerie.CalculateEMA((int)this.Parameters[0] / 4);
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
