using System;
using System.Drawing;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecorator_TOP2 : StockDecoratorBase, IStockDecorator
    {
        public override string Definition
        {
            get { return "Plots exhaustion points and divergences after waiting for price confirmation"; }
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
            get { return new string[] { "Period" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames { get { return new string[] { }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { };
                }
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            CreateEventSeries(stockSerie.Count);

            IStockIndicator indicator = stockSerie.GetIndicator(this.DecoratedItem);

            int upTrendLength = 0;
            int downTrendLength = 0;

            if (indicator == null || indicator.Series[0].Count == 0) return;
            FloatSerie indicatorToDecorate = indicator.Series[0];

            int period = (int)this.parameters[0];

            for (int i = 1; i < indicatorToDecorate.Count; i++)
            {
                if (indicatorToDecorate.IsTop(i - 1))
                {
                    if (upTrendLength > period)
                    {
                        this.eventSeries[0][i] = true;
                    }
                    upTrendLength = 0;
                    downTrendLength = 1;
                }
                else if (indicatorToDecorate.IsBottom(i - 1))
                {
                    if (downTrendLength > period)
                    {
                        this.eventSeries[1][i] = true;
                    }
                    upTrendLength = 1;
                    downTrendLength = 0;
                }
                else if (upTrendLength > 0)
                {
                    upTrendLength++;
                }
                else
                {
                    downTrendLength++;
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
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}

