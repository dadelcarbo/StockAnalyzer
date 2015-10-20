using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   /// <summary>
   /// Cummulative RSI with slow signal
   /// </summary>
   public class StockIndicator_CHOP : StockIndicatorBase
   {
      public StockIndicator_CHOP()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override string Definition
      {
         get { return "CHOP(int Period, int Smoothing)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Smooting" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 20, 20 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }

      public override string[] SerieNames { get { return new string[] { "CHOP(" + this.Parameters[0].ToString() + ")", "SlowCHOP(" + this.Parameters[1].ToString() + ")" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkRed) };
            }
            return seriePens;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
         FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

         FloatSerie chopSerie = new FloatSerie(stockSerie.Count);
         int period = (int) this.parameters[0];

         for (int i = period; i < stockSerie.Count; i++)
         {
            float low = float.MaxValue;
            float high = float.MinValue;
            float dist = 0f;
            for (int j = i - period; j <= i; j++)
            {
               low = Math.Min(low, lowSerie[j]);
               high = Math.Max(high, highSerie[j]);
               dist += highSerie[j] - lowSerie[j];
            }
            chopSerie[i] = dist/(high - low);
         }
         chopSerie = chopSerie.CalculateEMA((int) this.parameters[1]);
         this.series[0] = chopSerie;
         this.series[0].Name = this.Name;

         FloatSerie signal = chopSerie.CalculateEMA((int)this.parameters[1]);
         this.series[1] = signal;
         this.series[1].Name = this.SerieNames[1];

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (int i = 1; i < stockSerie.Count; i++)
         {
            this.eventSeries[0][i] = chopSerie[i] >= signal[i];
            this.eventSeries[1][i] = chopSerie[i] < signal[i];
            this.eventSeries[2][i] = (chopSerie[i - 1] < signal[i - 1] && chopSerie[i] >= signal[i]);
            this.eventSeries[3][i] = (chopSerie[i - 1] >= signal[i - 1] && chopSerie[i] < signal[i]);
         }
      }


      static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BullishCrossing", "BearishCrossing" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { false, false, true, true };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
