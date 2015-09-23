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
        public override string Name
        {
            get { return "TOP"; }
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

        public override string[] SerieNames { get { return new string[] { "Top", "Bottom" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green), new Pen(Color.Red) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            IStockIndicator indicator = stockSerie.GetIndicator(this.DecoratedItem);
            if (indicator != null && indicator.Series[0].Count > 0)
            {
                for (int i = 0; i < this.SeriesCount; i++)
                {
                    this.Series[i] = new BoolSerie(stockSerie.Count, this.SerieNames[i]);
                }
                FloatSerie indicatorToDecorate = indicator.Series[0];

                for (int i = 1; i < indicatorToDecorate.Count - 2; i++)
                {
                    if (indicatorToDecorate.IsTop(i))
                    {
                        this.Series[0][i] = true;
                    }
                    else if (indicatorToDecorate.IsBottom(i))
                    {
                        this.Series[1][i] = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < this.SeriesCount; i++)
                {
                    this.Series[i] = new BoolSerie(0, this.SerieNames[i]);
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

