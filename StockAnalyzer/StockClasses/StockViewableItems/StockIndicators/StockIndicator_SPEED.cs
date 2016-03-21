using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_SPEED : StockIndicatorBase
   {
      public StockIndicator_SPEED()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override object[] ParameterDefaultValues
      {
         get { return new Object[] { "ER(20_1_1)", 12 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeIndicator(), new ParamRangeInt(2, 500)   }; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Indicator", "Period" }; }
      }

      public override string[] SerieNames { get { return new string[] { "SPEED(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

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
         FloatSerie indicatorSerie = stockSerie.GetIndicator(this.parameters[0].ToString()).Series[0];
         FloatSerie speedSerie = indicatorSerie - indicatorSerie.CalculateEMA((int) this.parameters[1]);

         this.series[0] = speedSerie;
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
