using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_PIVOTLines : StockIndicatorBase
   {
      public StockIndicator_PIVOTLines()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.PriceIndicator; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Period", "Smoothing" }; }
      }
      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { 1, 1 }; }
      }
      public override ParamRange[] ParameterRanges
      {
         get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
      }
      public override string[] SerieNames { get { return new string[] { "PIVOT", "S1", "S2", "S3", "R1", "R2", "R3" }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] { new Pen(Color.Black), new Pen(Color.Green), new Pen(Color.Blue), new Pen(Color.Red), new Pen(Color.Green), new Pen(Color.Blue), new Pen(Color.Red) };
            }
            return seriePens;
         }
      }
      public override void ApplyTo(StockSerie stockSerie)
      {
         int period = (int)this.Parameters[0];
         int smoothing = (int)this.Parameters[1];
         FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW).CalculateEMA(smoothing);
         FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH).CalculateEMA(smoothing);
         FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE).CalculateEMA(smoothing);


         FloatSerie pivotSerie = new FloatSerie(stockSerie.Count, "PIVOT");
         FloatSerie s1Serie = new FloatSerie(stockSerie.Count, "S1");
         FloatSerie s2Serie = new FloatSerie(stockSerie.Count, "S2");
         FloatSerie s3Serie = new FloatSerie(stockSerie.Count, "S3");
         FloatSerie r1Serie = new FloatSerie(stockSerie.Count, "R1");
         FloatSerie r2Serie = new FloatSerie(stockSerie.Count, "R2");
         FloatSerie r3Serie = new FloatSerie(stockSerie.Count, "R3");

         this.Series[0] = pivotSerie;
         this.Series[1] = s1Serie;
         this.Series[2] = s2Serie;
         this.Series[3] = s3Serie;
         this.Series[4] = r1Serie;
         this.Series[5] = r2Serie;
         this.Series[6] = r3Serie;

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         if (stockSerie.StockName.StartsWith("INT_"))
         {
            List<StockDailyValue> dailyValues = stockSerie.GenerateDailyFromIntraday();
            IEnumerator<StockDailyValue> dailyEnumerator = dailyValues.GetEnumerator();

            dailyEnumerator.Reset();
            dailyEnumerator.MoveNext();
            StockDailyValue dailyValue = dailyEnumerator.Current;

            float pivot;
            float s1;
            float r1;
            float r2;
            float s2;
            float r3;
            float s3;

            dailyValue.CalculatePivot(out pivot, out s1, out r1, out r2, out s2, out r3, out s3);

            DateTime intradayBarDate = stockSerie.Values.First().DATE.Date;

            int count = 0;
            bool first = true;
            foreach (StockDailyValue intradayValue in stockSerie.Values)
            {
               if (intradayBarDate != intradayValue.DATE.Date)
               {
                  if (first)
                  {
                     first = false;
                  }
                  else
                  {
                     dailyEnumerator.MoveNext();
                     dailyEnumerator.Current.CalculatePivot(out pivot, out s1, out r1, out r2, out s2, out r3,
                         out s3);
                  }
                  intradayBarDate = intradayValue.DATE.Date;
               }

               pivotSerie[count] = pivot;
               r1Serie[count] = r1;
               s1Serie[count] = s1;

               r2Serie[count] = r2;
               s2Serie[count] = s2;

               r3Serie[count] = r3;
               s3Serie[count] = s3;

               count++;
            }
         }
         else
         {
            for (int i = 0; i <= period; i++)
            {
               s1Serie[i] = closeSerie[i];
               s2Serie[i] = closeSerie[i];
               s3Serie[i] = closeSerie[i];
               r1Serie[i] = closeSerie[i];
               r2Serie[i] = closeSerie[i];
               r3Serie[i] = closeSerie[i];
            }
            for (int i = period + 1; i < stockSerie.Count; i++)
            {
               float low = lowSerie.GetMin(i - period - 1, i - 1);
               float high = highSerie.GetMax(i - period - 1, i - 1);
               float pivot = (low + high + closeSerie[i - 1]) / 3;

               pivotSerie[i] = pivot;
               r1Serie[i] = (2 * pivot) - low;
               s1Serie[i] = (2 * pivot) - high;

               r2Serie[i] = (pivot - s1Serie[i]) + r1Serie[i];
               s2Serie[i] = pivot - (r1Serie[i] - s1Serie[i]);

               r3Serie[i] = (pivot - s2Serie[i]) + r2Serie[i];
               s3Serie[i] = pivot - (r2Serie[i] - s2Serie[i]);
            }
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
