using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
   public class StockIndicator_BREADTH : StockIndicatorBase, IRange
   {
      public StockIndicator_BREADTH()
      {
      }
      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.RangedIndicator; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Breadth", "FastSmooting" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { "MYOSC_20.CAC40", "1" }; }
      }
      static List<string> breadthSeries = null;
      public override ParamRange[] ParameterRanges
      {
         get
         {
            if (breadthSeries == null)
            {
               breadthSeries = StockDictionary.StockDictionarySingleton.Values.Where(v => v.BelongsToGroup(StockSerie.Groups.BREADTH)).Select(v => v.StockName).ToList();
            }
            return new ParamRange[]
            {
                new ParamRangeStringList( breadthSeries),
                new ParamRangeInt(1,500)
            };
         }
      }

      public override string[] SerieNames { get { return new string[] { (string)this.parameters[0] }; } }

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

      public override void ApplyTo(StockSerie stockSerie)
      {
         // Calculate Bollinger Bands
         FloatSerie breadthIndicator = new FloatSerie(stockSerie.Count);
         int fastSmoothing = (int)this.parameters[1];

         StockSerie breadthSerie = StockDictionary.StockDictionarySingleton[(string)this.parameters[0]];
         breadthSerie.Initialise();

         int index = -1;
         int i = 0;
         float breadth = 0;
         foreach (StockDailyValue dailyValue in stockSerie.Values)
         {
            if ((index = breadthSerie.IndexOf(dailyValue.DATE)) == -1)
            {
               breadthIndicator[i] = breadth;
            }
            else
            {
               breadthIndicator[i] = breadth = breadthSerie.ElementAt(index).Value.CLOSE;
            }
            i++;
         }

         breadthIndicator = breadthIndicator.CalculateEMA(fastSmoothing);

         this.series[0] = breadthIndicator;
         this.Series[0].Name = this.SerieNames[0];

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

         for (i = 1; i < stockSerie.Count; i++)
         {
            if (breadthIndicator[i] >= 0)
            {
               this.Events[0][i] = true;
            }
            else
            {
               this.Events[1][i] = true;
            }
            if (breadthIndicator[i] >= breadthIndicator[i - 1])
            {
               this.Events[2][i] = true;
            }
            else
            {
               this.Events[3][i] = true;
            }
         }
      }

      static string[] eventNames = new string[] { "Positive", "Negative", "Bullish", "Bearish"};
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] {false,false,false,false };
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }

      public float Max
      {
         get { return 1.0f; }
      }

      public float Min
      {
         get { return -1.0f; }
      }
   }
}
