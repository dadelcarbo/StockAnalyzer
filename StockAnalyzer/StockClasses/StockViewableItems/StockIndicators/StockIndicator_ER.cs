using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_ER : StockIndicatorBase
   {
      public StockIndicator_ER()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override object[] ParameterDefaultValues
      {
         get { return new Object[] { 20, 1, 1 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "InputSmoothing", "Smoothing" }; }
      }

      public override string[] SerieNames { get { return new string[] { "ER(" + this.Parameters[0].ToString() + ")" }; } }

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
         get
         {
            HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.Gray)) };
            lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            return lines;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         int period = Math.Min((int) this.parameters[0], stockSerie.Count - 1);
         int inputSmoothing = (int) this.parameters[1];
         int smoothing = (int) this.parameters[2];
         this.series[0] = stockSerie.CalculateER(period, inputSmoothing).CalculateEMA(smoothing);
         this.Series[0].Name = this.Name;
         this.CreateEventSeries(stockSerie.Count);

         for (int i = period; i < stockSerie.Count; i++)
         {
            this.eventSeries[0][i] = this.series[0][i] > 0;
            this.eventSeries[1][i] = this.series[0][i] < 0;
            this.eventSeries[2][i] = this.series[0][i] > 0 && this.series[0][i - 1] < 0;
            this.eventSeries[3][i] = this.series[0][i] < 0 && this.series[0][i - 1] > 0;
         }
      }

      static string[] eventNames = new string[] { "Positive", "Negative", "TurnedPositive", "TurnedNegative" };
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
