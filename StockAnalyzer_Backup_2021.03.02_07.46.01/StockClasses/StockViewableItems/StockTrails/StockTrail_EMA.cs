using System;
using System.Drawing;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrails
{
   public class StockTrail_EMA : StockTrailBase, IStockTrail
   {
      public override string Name
      {
         get { return "EMA(" + this.Parameters[0].ToString() + ")"; }
      }

      public override string Definition
      {
         get { return "Draws a trail based on EMA indicator"; }
      }

      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override IndicatorDisplayStyle DisplayStyle
      {
         get { return IndicatorDisplayStyle.TrailCurve; }
      }

      public override object[] ParameterDefaultValues
      {
         get { return new Object[] { 10 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period" }; }
      }


      public override string[] SerieNames { get { return new string[] { "Trail" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.DarkRed) };
            }
            return seriePens;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         IStockIndicator indicator = stockSerie.GetIndicator(this.TrailedItem);
         if (indicator != null && indicator.Series[0].Count > 0)
         {
            FloatSerie indicatorSerie = indicator.Series[0];
            FloatSerie trailSerie = indicatorSerie.CalculateEMATrailStop((int)this.parameters[0],1);
            this.Series[0] = trailSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = (int)this.parameters[0]; i < stockSerie.Count; i++)
            {
               int j = 0;
               this.eventSeries[j++][i] = indicatorSerie[i] >= trailSerie[i];
               this.eventSeries[j++][i] = indicatorSerie[i] < trailSerie[i]; ;
               this.eventSeries[j++][i] = indicatorSerie[i] >= trailSerie[i] && indicatorSerie[i - 1] < trailSerie[i - 1];
               this.eventSeries[j++][i] = indicatorSerie[i] < trailSerie[i] && indicatorSerie[i - 1] > trailSerie[i - 1];
               this.eventSeries[j++][i] = indicatorSerie[i] > indicatorSerie[i - 1];
               this.eventSeries[j++][i] = indicatorSerie[i] < indicatorSerie[i - 1];
            }

         }
         else
         {
            this.Series[0] = new FloatSerie(0, this.SerieNames[0]);
         }
      }
      static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BrokenUp", "BrokenDown", "TrailedUp", "TrailedDown" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}

