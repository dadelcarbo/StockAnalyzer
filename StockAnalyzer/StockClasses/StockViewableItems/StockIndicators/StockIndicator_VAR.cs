using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_VAR : StockIndicatorBase
   {
      public StockIndicator_VAR()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override string Name
      {
         get { return "VAR(" + this.Parameters[0].ToString() + ")"; }
      }

      public override string Definition
      {
         get { return "VAR(int Period)"; }
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

      public override string[] SerieNames { get { return new string[] { "VAR(" + this.Parameters[0].ToString() + ")" }; } }

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

      public override void ApplyTo(StockSerie stockSerie)
      {
         FloatSerie varSerie = stockSerie.GetSerie(StockDataType.VARIATION);
         FloatSerie emaSerie = varSerie.CalculateEMA( (int)this.parameters[0]);

         this.series[0] = emaSerie;
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
