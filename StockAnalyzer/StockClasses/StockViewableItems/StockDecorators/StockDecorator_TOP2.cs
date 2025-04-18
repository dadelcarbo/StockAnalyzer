﻿using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecorator_TOP2 : StockDecoratorBase, IStockDecorator
    {
        public override string Definition => "Plots exhaustion points and divergences after waiting for price confirmation";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.DecoratorPlot;

        public override string[] ParameterNames => new string[] { "Period" };
        public override Object[] ParameterDefaultValues => new Object[] { 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { };

        public override Pen[] SeriePens
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
        public override Pen[] EventPens
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

