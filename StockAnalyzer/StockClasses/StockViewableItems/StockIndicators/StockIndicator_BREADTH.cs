using System;
using System.Drawing;
using StockAnalyzer.StockMath;
using System.Collections.Generic;
using System.Linq;

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
      public override string Definition
      {
         get { return "BREADTH(string Breadth)"; }
      }
      public override string[] ParameterNames
      {
         get { return new string[] { "Breadth" }; }
      }

      public override Object[] ParameterDefaultValues
      {
         get { return new Object[] { "HLEMA5_6.SP500" }; }
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
                new ParamRangeStringList( breadthSeries)
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

         StockSerie breadthSerie = StockDictionary.StockDictionarySingleton[(string)this.parameters[0]];

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

         this.series[0] = breadthIndicator;
         this.Series[0].Name = this.SerieNames[0];

         // Detecting events
         this.CreateEventSeries(stockSerie.Count);

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

      public float Max
      {
         get {return 1.0f; }
      }

      public float Min
      {
         get { return 0f; }
      }
   }
}
