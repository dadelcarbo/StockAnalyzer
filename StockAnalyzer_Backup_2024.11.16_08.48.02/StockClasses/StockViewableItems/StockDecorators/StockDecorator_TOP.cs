using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecorator_TOP : StockDecoratorBase, IStockDecorator
    {
        public StockDecorator_TOP()
        {
        }

        public override string Definition => "Plots maximums and minimums";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.DecoratorPlot;

        public override string[] ParameterNames => new string[] { };
        public override Object[] ParameterDefaultValues => new Object[] { };
        public override ParamRange[] ParameterRanges => new ParamRange[] { };

        public override string[] SerieNames => new string[] { };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { };
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

                for (int i = 10; i < indicatorToDecorate.Count; i++)
                {
                    if (indicatorToDecorate.IsTop(i - 1))
                    {
                        this.eventSeries[0][i] = true;
                    }
                    else if (indicatorToDecorate.IsBottom(i - 1))
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
                eventPens ??= new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                return eventPens;
            }
        }

        static readonly string[] eventNames = new string[] { "Top", "Bottom" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}

