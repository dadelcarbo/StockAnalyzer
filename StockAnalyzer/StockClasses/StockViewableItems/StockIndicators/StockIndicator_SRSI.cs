using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_SRSI : StockIndicatorBase, IRange
   {
      public StockIndicator_SRSI()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.RangedIndicator; }
      }
      public override string Name
      {
         get { return "SRSI(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[2].ToString() + ")"; }
      }
      public override string Definition
      {
         get { return "SRSI(int Period, int FastSmoothing, int SlowSmoothing)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "FastSmoothing", "SlowSmoothing" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 12, 6, 6 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }

      public override string[] SerieNames { get { return new string[] { "SRSI(" + this.Parameters[0].ToString() + ")", "SRSI_SLOW(" + this.Parameters[1].ToString() + ")" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
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
         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         FloatSerie srsiSerie;
         int period = (int)this.parameters[0];
         if (closeSerie.Min <= 0.0f)
         {
            srsiSerie = closeSerie.CalculateRSI(period, false);
         }
         else
         {
            srsiSerie = closeSerie.CalculateRSI(period, true);
         }

         srsiSerie = srsiSerie - 50.0f;
         srsiSerie = srsiSerie.CalculateSigmoid(100f, 0.1f);

         this.series[0] = srsiSerie.CalculateEMA((int)this.parameters[1]);
         this.series[0].Name = this.SerieNames[0];

         this.series[1] = srsiSerie.CalculateEMA((int)this.parameters[2]);
         this.series[1].Name = this.SerieNames[1];


         //// 
         //srsiSerie = srsiSerie.CalculateStochastik((int)this.parameters[0], (int)this.parameters[1]).CalculateFisher((int)this.parameters[1]);

         ////srsiSerie = srsiSerie.CalculateEMA(period);
         //srsiSerie = srsiSerie.CalculateFisherInv(1);

         //this.series[0] = srsiSerie.CalculateFisherInv(1);
         //this.series[0].Name = this.SerieNames[0];

         //this.series[1] = srsiSerie.CalculateEMA((int)this.parameters[2]);
         //this.series[1].Name = this.SerieNames[1];

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (int i = 2; i < srsiSerie.Count; i++)
         {
            this.eventSeries[0][i] = (srsiSerie[i - 2] < srsiSerie[i - 1] && srsiSerie[i - 1] > srsiSerie[i]);
            this.eventSeries[1][i] = (srsiSerie[i - 2] > srsiSerie[i - 1] && srsiSerie[i - 1] < srsiSerie[i]);
            this.eventSeries[2][i] = this.eventSeries[0][i] && !this.eventSeries[0][i - 1];
            this.eventSeries[3][i] = this.eventSeries[1][i] && !this.eventSeries[1][i - 1];
         }
      }

      static string[] eventNames = new string[] { "Top", "Bottom", "BullishCrossing", "BearishCrossing" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true, true, true };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }

      public float Max
      {
         get { return 100.0f; }
      }

      public float Min
      {
         get { return -100.0f; }
      }
   }
}
