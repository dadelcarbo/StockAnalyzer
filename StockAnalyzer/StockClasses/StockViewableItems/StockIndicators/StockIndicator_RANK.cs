using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_RANK : StockIndicatorBase, IRange
   {
      public StockIndicator_RANK()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.RangedIndicator; }
      }
      public override string Name
      {
         get { return "RANK(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"; }
      }
      public override string Definition
      {
         get { return "RANK(int Period1, int Period2, float Overbought, float Oversold)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period1", "Period2", "Overbought", "Oversold" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 1, 10, -75f, 75f }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(-100f, 100f), new ParamRangeFloat(-100f, 100f) }; }
      }

      public override string[] SerieNames { get { return new string[] { "RANK", "RANK(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

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
      public override HLine[] HorizontalLines
      {
         get
         {
            HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)), new HLine((float)this.parameters[2], new Pen(Color.Gray)), new HLine((float)this.parameters[3], new Pen(Color.Gray)) };
            lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            return lines;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         FloatSerie rankSerie = stockSerie.GetSerie(StockDataType.POSITION);

         this.series[0] = rankSerie.CalculateEMA((int)this.Parameters[0]);
         this.series[0].Name = this.SerieNames[0];

         this.series[1] = rankSerie.CalculateHLTrail((int)this.Parameters[1]);
         this.series[1].Name = this.SerieNames[1];

         //
         float overbought = (float)this.Parameters[2];
         float oversold = (float)this.Parameters[3];
         for (int i = (int)this.Parameters[1]; i < stockSerie.Count; i++)
         {
            float rank = this.series[0][i];
            float rankSlow = this.series[1][i];
            this.eventSeries[0][i] = (rank > overbought);
            this.eventSeries[1][i] = (rank > oversold);
            this.eventSeries[2][i] = (rank > rankSlow);
            this.eventSeries[3][i] = (rank < rankSlow);
         }
      }

      public float Max
      {
         get { return 100.0f; }
      }

      public float Min
      {
         get { return -100.0f; }
      }

      static string[] eventNames = new string[] { "Overbought", "Oversold", "Bullish", "Bearish" };
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
