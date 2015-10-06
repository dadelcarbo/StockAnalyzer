using System;
using System.Drawing;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
   public class StockDecorator_TOP : StockDecoratorBase, IStockDecorator
   {
      public StockDecorator_TOP()
      {
      }
      
      public override string Definition
      {
         get { return "Plots maximums and minimums"; }
      }

      public override IndicatorDisplayTarget DisplayTarget
      {
         get { return IndicatorDisplayTarget.NonRangedIndicator; }
      }

      public override IndicatorDisplayStyle DisplayStyle
      {
         get { return IndicatorDisplayStyle.DecoratorPlot; }
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

      public override string[] SerieNames { get { return new string[] { }; } }

      public override System.Drawing.Pen[] SeriePens
      {
         get
         {
            if (seriePens == null)
            {
               seriePens = new Pen[] {  };
            }
            return seriePens;
         }
      }

      public override void ApplyTo(StockSerie stockSerie)
      {
         CreateEventSeries(stockSerie.Count);

         IStockIndicator indicator = stockSerie.GetIndicator(this.DecoratedItem);
         if (indicator != null && indicator.Series[0].Count > 0)
         {
            FloatSerie indicatorToDecorate = indicator.Series[0];

            for (int i = 10; i < indicatorToDecorate.Count - 2; i++)
            {
               if (indicatorToDecorate.IsTop(i-1))
               {
                  this.eventSeries[0][i] = true;
               }
               else if (indicatorToDecorate.IsBottom(i-1))
               {
                  this.eventSeries[1][i] = true;
               }
            }
         }
      }

      public override System.Drawing.Pen[] EventPens
      {
         get
         {
            if (eventPens == null)
            {
               eventPens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
            }
            return eventPens;
         }
      }

      static string[] eventNames = new string[] { "Top", "Bottom" };
      public override string[] EventNames
      {
         get { return eventNames; }
      }
      static readonly bool[] isEvent = new bool[] { true,true};
      public override bool[] IsEvent
      {
         get { return isEvent; }
      }
   }
}

