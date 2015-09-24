using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_BUYMOM : StockIndicatorBase
   {
      public StockIndicator_BUYMOM()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override bool RequiresVolumeData { get { return true; } }

      public override string Definition
      {
         get { return "BUYMOM(int Period, bool UseLog)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "UseLog" }; }
      }
      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 12, false }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeBool() }; }
      }
      public override string[] SerieNames { get { return new string[] { "FASTMOM", "SLOWMOM" }; } }

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
            HLine[] lines = new HLine[] { new HLine(0, new Pen(Color.Gray)) };
            lines[0].LinePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            return lines;
         }
      }
      public override void ApplyTo(StockSerie stockSerie)
      {
         if (!stockSerie.HasVolume)
         {
            return;
         }
         FloatSerie fastMom = stockSerie.CalculateBuySellMomemtum((int)this.parameters[0], (bool)this.parameters[1]);
         this.series[0] = fastMom;
         this.series[0].Name = this.SerieNames[0];
         FloatSerie slowMom = fastMom.CalculateEMA((int)this.parameters[0] / 2);
         this.series[1] = slowMom;
         this.series[1].Name = this.SerieNames[1];

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (int i = 1; i < stockSerie.Count; i++)
         {
            this.eventSeries[0][i] = (slowMom[i - 1] > fastMom[i - 1] && slowMom[i] < fastMom[i]);
            this.eventSeries[1][i] = (slowMom[i - 1] < fastMom[i - 1] && slowMom[i] > fastMom[i]);
         }
      }

      static string[] eventNames = new string[] { "BullishCrossing", "BearishCrossing" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
