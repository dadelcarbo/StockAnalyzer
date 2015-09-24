using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_RSI : StockIndicatorBase, IRange
   {
      public StockIndicator_RSI()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.RangedIndicator; }
      }
      public override string Name
      {
         get { return "RSI(" + this.Parameters[0].ToString() + ")"; }
      }
      public override string Definition
      {
         get { return "RSI(int Period, float Overbought, float Oversold)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Overbought", "Oversold" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 20, 75f, 25f }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 100f), new ParamRangeFloat(0f, 100f) }; }
      }

      public override string[] SerieNames { get { return new string[] { "RSI(" + this.Parameters[0].ToString() + ")" }; } }

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
      public override HLine[] HorizontalLines
      {
         get
         {
            HLine[] lines = new HLine[] { new HLine(50, new Pen(Color.LightGray)), new HLine((float)this.parameters[1], new Pen(Color.Gray)), new HLine((float)this.parameters[2], new Pen(Color.Gray)) };
            lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            lines[1].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            lines[2].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            return lines;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         FloatSerie rsiSerie;
         if (closeSerie.Min <= 0.0f)
         {
            rsiSerie = closeSerie.CalculateRSI((int)this.parameters[0], false);
         }
         else
         {
            rsiSerie = closeSerie.CalculateRSI((int)this.parameters[0], true);
         }
         this.series[0] = rsiSerie;
         this.series[0].Name = this.Name;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         float overbought = (float)this.parameters[1];
         float oversold = (float)this.parameters[2];

         for (int i = 2; i < rsiSerie.Count; i++)
         {
            this.eventSeries[0][i] = (rsiSerie[i - 2] < rsiSerie[i - 1] && rsiSerie[i - 1] > rsiSerie[i]);
            this.eventSeries[1][i] = (rsiSerie[i - 2] > rsiSerie[i - 1] && rsiSerie[i - 1] < rsiSerie[i]);
            this.eventSeries[2][i] = rsiSerie[i] >= overbought;
            this.eventSeries[3][i] = rsiSerie[i] <= oversold;
         }
      }

      public float Max
      {
         get { return 100.0f; }
      }

      public float Min
      {
         get { return 0.0f; }
      }

      static string[] eventNames = new string[] { "Top", "Bottom", "Overbought", "Oversold" };
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
