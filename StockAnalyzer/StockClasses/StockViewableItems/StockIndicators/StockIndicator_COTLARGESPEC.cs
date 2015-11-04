using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_COTLARGESPEC : StockIndicatorBase
   {
      public StockIndicator_COTLARGESPEC()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override string Name
      {
         get { return "COTLARGESPEC"; }
      }
      public override string Definition
      {
         get { return "COTLARGESPEC"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { }; }
      }
      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { }; }
      }

      public override string[] SerieNames { get { return new string[] { "COTLARGESPEC" }; } }

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
         if (stockSerie.CotSerie != null)
         {
            // Get the CotSerie with date consideration...
            this.series[0] = stockSerie.CotSerie.GetSerie(CotValue.CotValueType.LargeSpeculatorPosition, stockSerie.Keys.ToArray());
         }
         else
         {
            this.series[0] = new FloatSerie(0);
            return;
         }

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (int i = 2; i < stockSerie.Count; i++)
         {
         }
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
