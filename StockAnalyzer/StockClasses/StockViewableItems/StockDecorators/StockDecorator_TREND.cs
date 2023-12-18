using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecorator_TREND : StockDecoratorBase, IStockDecorator
    {
        public override string Definition => "Plots exhaustion points and divergences and provide signal events";

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.DecoratorPlot;

        public override string[] ParameterNames => new string[] { "FadeOut" };
        public override Object[] ParameterDefaultValues => new Object[] { 1.5f };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeFloat(0.1f, 10.0f) };

        public override string[] SerieNames => new string[] { "UpBand", "DownBand" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkGray), new Pen(Color.DarkGray), new Pen(Color.DarkRed) };

                    seriePens[1].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    seriePens[2].DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            using (MethodLogger ml = new MethodLogger(this))
            {
                CreateEventSeries(stockSerie.Count);
                IStockIndicator indicator = stockSerie.GetIndicator(this.DecoratedItem);

                if (indicator == null)
                    return;

                float fadeOut = (100.0f - (float)this.parameters[0]) / 100.0f;

                FloatSerie indicatorToDecorate = indicator.Series[0];
                FloatSerie upperLimit = new FloatSerie(indicatorToDecorate.Count);
                FloatSerie lowerLimit = new FloatSerie(indicatorToDecorate.Count);

                this.Series[0] = upperLimit;
                this.Series[0].Name = this.SerieNames[0];
                this.Series[1] = lowerLimit;
                this.Series[1].Name = this.SerieNames[1];


                upperLimit[0] = lowerLimit[0] = indicatorToDecorate[0];
                for (int i = 1; i < indicatorToDecorate.Count; i++)
                {
                    float value = indicatorToDecorate[i];
                    if (value > upperLimit[i - 1])
                    {
                        upperLimit[i] = value;
                        lowerLimit[i] = lowerLimit[i - 1] * fadeOut;
                        this.Events[0][i] = true;
                    }
                    else if (value < lowerLimit[i - 1])
                    {
                        upperLimit[i] = upperLimit[i - 1] * fadeOut;
                        lowerLimit[i] = value;
                        this.Events[1][i] = true;
                    }
                    else
                    {
                        lowerLimit[i] = lowerLimit[i - 1] * fadeOut;
                        upperLimit[i] = upperLimit[i - 1] * fadeOut;
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
                    eventPens[0].Width = 2;
                    eventPens[1].Width = 2;
                }
                return eventPens;
            }
        }

        static string[] eventNames = new string[] { "Bullish", "Bearish" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { true, true };
        public override bool[] IsEvent => isEvent;
    }
}