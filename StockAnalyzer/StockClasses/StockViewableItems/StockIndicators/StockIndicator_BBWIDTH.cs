using System;
using System.Collections.Generic;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_BBWIDTH : StockIndicatorBase
   {
      public StockIndicator_BBWIDTH()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }
      public override string Definition
      {
         get { return "BBWIDTH(int Period, string MAType)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "MAType" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 20, "MA" }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get
         {
            return new ParamRange[]
            {
                new ParamRangeInt(1, 500), 
                new ParamRangeStringList( new List<string>() {"EMA", "HMA", "MA", "EA"})
            };
         }
      }

      public override string[] SerieNames
      {
         get { return new string[] {"BBWIDTH"}; }
      }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] {new Pen(Color.Black)};
            }
            return seriePens;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         // Calculate Bollinger Bands
         FloatSerie upperBB = null;
         FloatSerie lowerBB = null;
         FloatSerie ema = stockSerie.GetIndicator(this.parameters[1] + "(" + (int)this.parameters[0] + ")").Series[0];

         stockSerie.GetSerie(StockDataType.CLOSE).CalculateBB(ema, (int)this.parameters[0], 1f, -1f, ref upperBB, ref lowerBB);

         FloatSerie widthLog = ((upperBB - lowerBB)/ema)*20.0f;
         for (int i = (int) this.parameters[0]; i < stockSerie.Count; i++)
         {
            widthLog[i] = (float)Math.Log10(widthLog[i]);
         }

         this.series[0] = widthLog;
         this.Series[0].Name = this.SerieNames[0];

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);
      }

      static string[] eventNames = new string[] {  };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] {  };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}
