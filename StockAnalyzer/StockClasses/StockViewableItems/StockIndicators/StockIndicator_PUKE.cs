using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_PUKE : StockIndicatorBase
   {
      public StockIndicator_PUKE()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override string Definition
      {
         get { return "PUKE(int Period, int Smoothing, float ATRPercent, int SignalSmoothing)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Smoothing", "ATRPercent", "SignalSmoothing" }; }
      }
      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 14, 3, 0.0f, 10 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeFloat(0.0f, 100.0f), new ParamRangeInt(1, 500) }; }
      }

      public override string[] SerieNames { get { return new string[] { "PUKE(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + "," + this.Parameters[2].ToString() + ")", "Signal" }; } }

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
      public override HLine[] HorizontalLines
      {
         get
         {
            HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.LightGray)) };
            lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            return lines;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         FloatSerie pukeSerie = new FloatSerie(stockSerie.Count);

         int period = ((int)this.parameters[0]);
         int smoothing = ((int)this.parameters[1]);
         float atrPercent = ((float)this.parameters[2]) / 100.0f;
         int signalSmoothing = ((int)this.parameters[3]);

         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         FloatSerie varSerie = stockSerie.GetSerie(StockDataType.CLOSE) - stockSerie.GetSerie(StockDataType.CLOSE);

         FloatSerie atrSerie = stockSerie.GetIndicator("ATR(" + this.parameters[0] + ")").Series[0] * atrPercent;

         var values = stockSerie.ValueArray;

         // Calculate PUKE
         for (int i = period; i < stockSerie.Count; i++)
         {

            int puke = 0;
            for (int j = i - period; j <= i; j++)
            {
               StockDailyValue dv = values[j];
               float range = dv.CLOSE - dv.PreviousClose;
               if (range >= atrSerie[j]) puke++;
               else if (range <= -atrSerie[j]) puke--;
            }
            pukeSerie[i] = puke;
         }

         if (smoothing > 1)
         {
            pukeSerie = pukeSerie.CalculateEMA(smoothing);
         }
         this.Series[0] = pukeSerie;
         this.series[0].Name = this.SerieNames[0];

         FloatSerie signalSerie = pukeSerie.CalculateEMA(signalSmoothing);
         this.series[1] = signalSerie;
         this.series[1].Name = this.SerieNames[1];

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (int i = period; i < stockSerie.Count; i++)
         {
            this.eventSeries[0][i] = (signalSerie[i - 1] > pukeSerie[i - 1] && signalSerie[i] < pukeSerie[i]);
            this.eventSeries[1][i] = (signalSerie[i - 1] < pukeSerie[i - 1] && signalSerie[i] > pukeSerie[i]);
            this.eventSeries[2][i] = pukeSerie[i] > signalSerie[i];
            this.eventSeries[3][i] = pukeSerie[i] < signalSerie[i];
         }
      }

      static string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing", "Bullish", "Bearish" };
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
