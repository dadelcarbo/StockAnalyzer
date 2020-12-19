using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_AUTOCORREL : StockIndicatorBase, IRange
   {
      public StockIndicator_AUTOCORREL()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.RangedIndicator; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Shift" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 100, 6 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 1000), new ParamRangeInt(0, 500) }; }
      }
      public override string[] SerieNames { get { return new string[] { "AUTOCORREL(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")" }; } }


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
         FloatSerie varSerie = stockSerie.GetSerie(StockDataType.CLOSE);
         FloatSerie autoCorrelationSerie = varSerie.CalculateAutoCorrelation((int)this.parameters[0], (int)this.parameters[1]);
         this.series[0] = autoCorrelationSerie;
         this.Series[0].Name = this.Name;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (int i = 2; i < stockSerie.Count; i++)
         {
            this.eventSeries[0][i] = (varSerie[i - 2] < varSerie[i - 1] && varSerie[i - 1] > varSerie[i]);
            this.eventSeries[1][i] = (varSerie[i - 2] > varSerie[i - 1] && varSerie[i - 1] < varSerie[i]);
            this.eventSeries[2][i] = (varSerie[i - 1] < 0 && varSerie[i] >= 0);
            this.eventSeries[3][i] = (varSerie[i - 1] > 0 && varSerie[i] <= 0);
         }
      }

      static string[] eventNames = new string[] { "Top", "Bottom", "TurnedPositive", "TurnedNegative" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true, true, true, true };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }

        public float Max => 1f;

        public float Min => -1f;
    }
}
