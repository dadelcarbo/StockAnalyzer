using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecorator_TARGET : StockDecoratorBase, IStockDecorator
    {
        public override string Definition
        {
            get { return "Plots provide signal events based on oversold/overbought levels"; }
        }

        public StockDecorator_TARGET()
        {
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
            get { return new string[] { "Overbought", "Oversold" }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 75f, 25f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeFloat(0.0f, 1000.0f), new ParamRangeFloat(-1000.0f, 1000.0f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "Data" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Black) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                List<string> eventNames = this.EventNames.ToList();
                int overboughtIndex = eventNames.IndexOf("Overbought");
                int oversoldIndex = eventNames.IndexOf("Oversold");
                int beginOverboughtIndex = eventNames.IndexOf("BeginOverbought");
                int beginOversoldIndex = eventNames.IndexOf("BeginOversold");
                int endOverboughtIndex = eventNames.IndexOf("EndOverbought");
                int endOversoldIndex = eventNames.IndexOf("EndOversold");

                CreateEventSeries(stockSerie.Count);

                float overboughtTrigger = (float)this.parameters[0];
                float oversoldTrigger = (float)this.parameters[1];

                IStockIndicator indicator = stockSerie.GetIndicator(this.DecoratedItem);
                if (indicator == null || indicator.Series[0].Count == 0)
                    return;
                FloatSerie indicatorToDecorate = indicator.Series[0];

                this.Series[0] = indicatorToDecorate;
                this.Series[0].Name = this.SerieNames[0];

                for (int i = 1; i < indicatorToDecorate.Count - 1; i++)
                {
                    bool overbought = indicatorToDecorate[i] > overboughtTrigger;
                    bool oversold = indicatorToDecorate[i] < oversoldTrigger;
                    this.eventSeries[overboughtIndex][i] = overbought;
                    this.eventSeries[oversoldIndex][i] = oversold;
                    this.eventSeries[beginOverboughtIndex][i] = overbought && indicatorToDecorate[i - 1] < overboughtTrigger;
                    this.eventSeries[beginOversoldIndex][i] = oversold && indicatorToDecorate[i - 1] > oversoldTrigger;
                    this.eventSeries[endOverboughtIndex][i] = !overbought && indicatorToDecorate[i - 1] > overboughtTrigger;
                    this.eventSeries[endOversoldIndex][i] = !oversold && indicatorToDecorate[i - 1] < oversoldTrigger;
                }
            }
        }

        public override System.Drawing.Pen[] EventPens
        {
            get
            {
                if (eventPens == null)
                {
                    eventPens = new Pen[] {
                        new Pen(Color.Green), new Pen(Color.Red),
                        new Pen(Color.Green), new Pen(Color.Red),
                        new Pen(Color.Red), new Pen(Color.Green) };
                    eventPens[0].Width = 2;
                    eventPens[1].Width = 2;
                }
                return eventPens;
            }
        }

        private static string[] eventNames = new string[]
        {
            "Overbought", "Oversold",
            "BeginOverbought", "BeginOversold",
            "EndOverbought", "EndOversold"
        };

        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[]
        {
            false, false,
            true, true,
            true, true
        };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
