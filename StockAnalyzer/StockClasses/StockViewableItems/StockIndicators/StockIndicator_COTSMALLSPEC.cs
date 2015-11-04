using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_COTSMALLSPEC : StockIndicatorBase
   {
      public StockIndicator_COTSMALLSPEC()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Smooting", "OpenIntPercent"}; }
      }
      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] {1, false }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1,500), new ParamRangeBool()};}
      }

      public override string[] SerieNames { get { return new string[] { "COTSMALLSPEC" }; } }

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
            int period = (int)this.parameters[0];
            bool percent = (bool)this.parameters[1];

            // Get the CotSerie with date consideration...
            FloatSerie cotSerie = stockSerie.CotSerie.GetSerie(CotValue.CotValueType.SmallSpeculatorPosition, stockSerie.Keys.ToArray());
            if (percent)
            {
               FloatSerie openInt = stockSerie.CotSerie.GetSerie(CotValue.CotValueType.OpenInterest, stockSerie.Keys.ToArray());
               cotSerie = cotSerie *100f/ openInt;
            }
            this.series[0] = cotSerie.CalculateEMA(period);
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
