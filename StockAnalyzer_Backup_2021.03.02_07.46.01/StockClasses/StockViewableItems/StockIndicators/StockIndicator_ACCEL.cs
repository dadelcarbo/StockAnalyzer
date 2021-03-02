using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_ACCEL : StockIndicatorBase
   {
      public StockIndicator_ACCEL()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period1", "Period2" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 12, 20 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }
      public override string[] SerieNames { get { return new string[] { "ACCEL(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }


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
               lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
            }
            return lines;
         }
      }
      public override void ApplyTo(StockSerie stockSerie)
      {
         IStockIndicator oscSerie = stockSerie.GetIndicator("OSC(" + this.parameters[0] + ","+ this.parameters[1] +")");

         FloatSerie fastSerie = oscSerie.Series[0].CalculateEMA((int)this.parameters[0]);
         FloatSerie slowSerie = oscSerie.Series[0].CalculateEMA((int)this.parameters[1]);

         FloatSerie accelSerie = fastSerie.Sub(slowSerie);
         
         this.series[0] = accelSerie;
         this.Series[0].Name = this.Name;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (int i = 2; i < stockSerie.Count; i++)
         {
            this.eventSeries[0][i] = (accelSerie[i - 2] < accelSerie[i - 1] && accelSerie[i - 1] > accelSerie[i]);
            this.eventSeries[1][i] = (accelSerie[i - 2] > accelSerie[i - 1] && accelSerie[i - 1] < accelSerie[i]);
            this.eventSeries[2][i] = (accelSerie[i - 1] < 0 && accelSerie[i] >= 0);
            this.eventSeries[3][i] = (accelSerie[i - 1] > 0 && accelSerie[i] <= 0);
            this.eventSeries[4][i] = accelSerie[i] >= 0;
            this.eventSeries[5][i] = accelSerie[i] < 0;
         }
      }

      static string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative", "Bullish", "Bearish" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
