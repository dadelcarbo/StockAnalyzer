using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_MYOSC : StockIndicatorBase
   {
      public StockIndicator_MYOSC()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override string Name
      {
         get { return "MYOSC(" + this.Parameters[0].ToString() + ")"; }
      }

      public override string Definition
      {
         get { return "MYOSC(int Period)"; }
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
      public override string[] SerieNames { get { return new string[] { "MYOSC(" + this.Parameters[0].ToString() + ")" }; } }

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
         FloatSerie fastK = stockSerie.CalculateFastOscillator((int)this.parameters[0]).Div(100.0f).Sub(0.5f);

         FloatSerie oscSerie = new FloatSerie(stockSerie.Count);

         int i = 0;
         StockDailyValue previousValue = null;
         float previousOSCValue = 0.0f;
         foreach (StockDailyValue dailyValue in stockSerie.Values)
         {
            if (previousValue != null)
            {
               if (fastK[i] > 0)
               {
                  previousOSCValue += (1.0f - previousOSCValue) * fastK[i];
               }
               else
               {
                  previousOSCValue -= (-1.0f - previousOSCValue) * fastK[i];
               }

               oscSerie[i] = previousOSCValue;
            }
            previousValue = dailyValue;
            i++;

         }


         this.series[0] = oscSerie;
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
