using System;
using System.Drawing;
using StockAnalyzer.StockMath;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_DISTHLS : StockIndicatorBase
   {
      public StockIndicator_DISTHLS()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Smooting" }; }
      }
      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 12, 6 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }

      public override string[] SerieNames { get { return new string[] { "DISTHLS(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }

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
         int smoothing = (int)this.parameters[1];

         IStockTrailStop trail = stockSerie.GetTrailStop("TRAILHLS(" + period + "," + smoothing + ")");
         FloatSerie longStop = trail.Series[0];
         FloatSerie shortStop = trail.Series[1];
         
         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(smoothing);

         FloatSerie distSerie = new FloatSerie(stockSerie.Count);
         for (int i = period + smoothing; i < stockSerie.Count; i++)
         {
            distSerie[i] = float.IsNaN(longStop[i]) ? closeSerie[i] - shortStop[i] : closeSerie[i] - longStop[i];
         }
         //cciSerie = cciSerie.CalculateSigmoid(100f, 0.02f).CalculateEMA((int)Math.Sqrt();
         //FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         //for (int i = 10; i < cciSerie.Count; i++)
         //{
         //    if (cciSerie[i] > overbought && cciSerie[i] <= cciSerie[i - 1] && closeSerie[i] >= closeSerie[i-1])
         //    {
         //        cciSerie[i] = cciSerie[i - 1] + (100 - cciSerie[i - 1]) / 4f;
         //    }
         //    else if (cciSerie[i] < oversold && cciSerie[i] >= cciSerie[i - 1] && closeSerie[i] <= closeSerie[i-1])
         //    {
         //        cciSerie[i] = cciSerie[i - 1] *0.75f;
         //    }
         //}

         this.series[0] = distSerie;
         this.series[0].Name = this.Name;

      }

      static string[] eventNames = new string[] { };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }

   }
}
