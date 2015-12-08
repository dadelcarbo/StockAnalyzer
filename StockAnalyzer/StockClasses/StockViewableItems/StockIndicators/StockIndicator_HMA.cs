using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_HMA : StockIndicatorBase
   {
      public StockIndicator_HMA()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }

      public override string Name
      {
         get { return "HMA(" + this.Parameters[0].ToString() + ")"; }
      }

      public override string Definition
      {
         get { return "HMA(int Period)"; }
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

      public override string[] SerieNames { get { return new string[] { "HMA(" + this.Parameters[0].ToString() + ")" }; } }

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
         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
         FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
         FloatSerie hmaSerie = closeSerie.CalculateHMA((int)this.parameters[0]);
         this.series[0] = hmaSerie;
         this.series[0].Name = this.Name;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);
         for (int i = 2; i < hmaSerie.Count; i++)
         {
            this.eventSeries[0][i] = (hmaSerie[i - 2] > hmaSerie[i - 1] && hmaSerie[i - 1] < hmaSerie[i]);
            this.eventSeries[1][i] = (hmaSerie[i - 2] < hmaSerie[i - 1] && hmaSerie[i - 1] > hmaSerie[i]);
            this.eventSeries[2][i] = closeSerie[i] > hmaSerie[i];
            this.eventSeries[3][i] = closeSerie[i] < hmaSerie[i];
            this.eventSeries[4][i] = lowSerie[i] > hmaSerie[i] && lowSerie[i - 1] < hmaSerie[i - 1];
            this.eventSeries[5][i] = highSerie[i] < hmaSerie[i] && highSerie[i - 1] > hmaSerie[i - 1];
         }
      }

      static string[] eventNames = new string[] { "Bottom", "Top", "CloseAbove", "CloseBelow", "BarAbove", "BarBelow" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true, false, false, true, true };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
