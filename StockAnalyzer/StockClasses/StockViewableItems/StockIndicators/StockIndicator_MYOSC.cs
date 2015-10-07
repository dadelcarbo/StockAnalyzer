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
      
      public override string Definition
      {
         get { return "MYOSC(int Period, int Signal)"; }
      }
      public override object[] ParameterDefaultValues
      {
         get { return new Object[] { 20, 6 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Signal" }; }
      }
      public override string[] SerieNames { get { return new string[] { "MYOSC(" + this.Parameters[0].ToString() + ")", "Signal" }; } }

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


         FloatSerie signalSerie = oscSerie.CalculateEMA((int)this.parameters[1]);
         this.series[1] = signalSerie;
         this.series[1].Name = this.series[0].Name + "_SIGNAL";

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);
         for (i = 2; i < oscSerie.Count; i++)
         {
            this.eventSeries[0][i] = (oscSerie[i] > signalSerie[i]);
            this.eventSeries[1][i] = (oscSerie[i] < signalSerie[i]);
            this.eventSeries[0][i] = eventSeries[0][i] & !eventSeries[0][i - 1];
            this.eventSeries[1][i] = eventSeries[1][i] & !eventSeries[1][i - 1];
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
