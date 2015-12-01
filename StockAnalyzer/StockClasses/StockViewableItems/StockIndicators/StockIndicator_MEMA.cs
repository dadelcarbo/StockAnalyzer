using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_MEMA : StockIndicatorBase
   {
      public StockIndicator_MEMA()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }
      public override object[] ParameterDefaultValues
      {
         get { return new Object[] { 3, 3}; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Iteration" }; }
      }
      public override string[] SerieNames { get { return new string[] { "MEMA(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

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
         int period = (int)this.parameters[0];
         int iteration = (int)this.parameters[1];

         FloatSerie memaSerie = closeSerie;
         for (int i = 0; i < iteration; i++)
         {
            memaSerie = memaSerie.CalculateEMA(period);
         }
            this.series[0] = memaSerie;
         this.series[0].Name = this.Name;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);
         for (int i = 2; i < memaSerie.Count; i++)
         {
            this.eventSeries[0][i] = (memaSerie[i - 2] > memaSerie[i - 1] && memaSerie[i - 1] < memaSerie[i]);
            this.eventSeries[1][i] = (memaSerie[i - 2] < memaSerie[i - 1] && memaSerie[i - 1] > memaSerie[i]);
            this.eventSeries[2][i] = closeSerie[i] > memaSerie[i];
            this.eventSeries[3][i] = closeSerie[i] < memaSerie[i];
         }
      }

      static string[] eventNames = new string[] { "Bottom", "Top", "PriceAbove", "PriceBelow" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true, false, false };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
