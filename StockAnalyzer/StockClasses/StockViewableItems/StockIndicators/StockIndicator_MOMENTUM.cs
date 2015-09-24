using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_MOMENTUM : StockIndicatorBase
   {
      public StockIndicator_MOMENTUM()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override string Definition
      {
         get { return "MOMENTUM(int Period, int Smoothing, int SignalSmoothing)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Smoothing", "SignalSmoothing" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 52, 3, 52 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }

      public override string[] SerieNames { get { return new string[] { "MOMENTUM(" + this.Parameters[0].ToString() + ")", "Signal(" + this.Parameters[2].ToString() + ")" }; } }

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
         int period = (int)this.parameters[0];
         int smooting = (int)this.parameters[1];
         int signalSmoothing = (int)this.parameters[2];

         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         if (smooting > 1) closeSerie = closeSerie.CalculateEMA(smooting);

         FloatSerie momentumSerie = new FloatSerie(stockSerie.Count);
         for (int i = period; i < stockSerie.Count; i++)
         {
            momentumSerie[i] = 100.0f * (closeSerie[i] - closeSerie[i - period]) / closeSerie[i - period];
         }

         FloatSerie signalSerie = momentumSerie.CalculateEMA(signalSmoothing);

         this.series[0] = momentumSerie;
         this.series[0].Name = this.SerieNames[0];

         this.series[1] = signalSerie;
         this.series[1].Name = this.SerieNames[1];

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);


         FloatSerie atrSerie = 100.0f * stockSerie.GetSerie(StockDataType.ATR).CalculateEMA(smooting) / closeSerie;

         for (int i = 2; i < stockSerie.Count; i++)
         {
            this.eventSeries[0][i] = (momentumSerie[i] - signalSerie[i]) >= atrSerie[i];
            this.eventSeries[1][i] = (momentumSerie[i] - signalSerie[i]) <= -atrSerie[i];
            this.eventSeries[2][i] = this.eventSeries[0][i] && !this.eventSeries[0][i - 1];
            this.eventSeries[3][i] = this.eventSeries[1][i] && !this.eventSeries[1][i - 1];
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
