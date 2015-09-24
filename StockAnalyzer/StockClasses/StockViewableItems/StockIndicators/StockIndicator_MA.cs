using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_MA : StockIndicatorBase
   {
      public StockIndicator_MA()
      {
      }

      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }

      public override string Name
      {
         get { return "MA(" + this.Parameters[0].ToString() + ")"; }
      }

      public override string Definition
      {
         get { return "MA(int Period)"; }
      }

      public override Object[] ParameterDefaultValues
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

      public override string[] SerieNames { get { return new string[] { "MA(" + this.Parameters[0].ToString() + ")" }; } }

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

      public override void ApplyTo(StockSerie stockSerie)
      {
         FloatSerie maSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateMA((int)this.parameters[0]);
         this.series[0] = maSerie;
         this.series[0].Name = this.Name;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);
         for (int i = 2; i < stockSerie.Count; i++)
         {
            this.eventSeries[0][i] = (maSerie[i - 2] > maSerie[i - 1] && maSerie[i - 1] < maSerie[i]);
            this.eventSeries[1][i] = (maSerie[i - 2] < maSerie[i - 1] && maSerie[i - 1] > maSerie[i]);
         }
      }

      static string[] eventNames = new string[] { "Bottom", "Top" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
