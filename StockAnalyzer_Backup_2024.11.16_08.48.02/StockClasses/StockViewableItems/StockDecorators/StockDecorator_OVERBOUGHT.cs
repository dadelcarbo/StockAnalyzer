using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockDecorators
{
    public class StockDecorator_OVERBOUGHT : StockDecoratorBase, IStockDecorator
    {
        public override string Definition => "Plots exhaustion points and divergences and provide signal events based on oversold/overbought levels";

        public StockDecorator_OVERBOUGHT()
        {
        }

        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.NonRangedIndicator;

        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.DecoratorPlot;

        public override string[] ParameterNames => new string[] { "Smoothing", "Overbought", "Oversold", "LookBack", "SignalSmoothing" };
        public override Object[] ParameterDefaultValues => new Object[] { 1, 0.75f, -0.75f, 30, 6 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0.0f, 1000.0f), new ParamRangeFloat(-1000.0f, 1000.0f), new ParamRangeInt(1, 500), new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "Data", "Overbought", "Oversold", "Signal" };

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
            using MethodLogger ml = new MethodLogger(this);
            List<string> eventNames = this.EventNames.ToList();
            int ExhaustionTopIndex = eventNames.IndexOf("ExhaustionTop");
            int ExhaustionBottomIndex = eventNames.IndexOf("ExhaustionBottom");
            int BearishDivergenceIndex = eventNames.IndexOf("BearishDivergence");
            int BullishDivergenceIndex = eventNames.IndexOf("BullishDivergence");
            int ExhaustionTopOccuredIndex = eventNames.IndexOf("ExhaustionTopOccured");
            int ExhaustionBottomOccuredIndex = eventNames.IndexOf("ExhaustionBottomOccured");
            int PositiveIndex = eventNames.IndexOf("Positive");
            int NegativeIndex = eventNames.IndexOf("Negative");
            int BullishIndex = eventNames.IndexOf("Bullish");
            int BearishIndex = eventNames.IndexOf("Bearish");
            int BearFailureIndex = eventNames.IndexOf("BearFailure");
            int BullFailureIndex = eventNames.IndexOf("BullFailure");

            CreateEventSeries(stockSerie.Count);

            int smoothing = (int)this.parameters[0];
            float overbought = (float)this.parameters[1];
            float oversold = (float)this.parameters[2];
            int lookbackPeriod = (int)this.parameters[3];
            int signalSmoothing = (int)this.parameters[4];

            int countNegative = 0;
            int countPositive = 0;

            IStockIndicator indicator = stockSerie.GetIndicator(this.DecoratedItem);
            if (indicator != null && indicator.Series[0].Count > 0)
            {
                FloatSerie indicatorToDecorate = indicator.Series[0].CalculateEMA(smoothing);
                FloatSerie signalSerie = indicatorToDecorate.CalculateEMA(signalSmoothing);
                FloatSerie upperLimit = new FloatSerie(indicatorToDecorate.Count); upperLimit.Reset(overbought);
                FloatSerie lowerLimit = new FloatSerie(indicatorToDecorate.Count); lowerLimit.Reset(oversold);

                if (smoothing <= 1) { this.SerieVisibility[0] = false; }
                if (signalSmoothing <= 1) { this.SerieVisibility[3] = false; }

                this.Series[0] = indicatorToDecorate;
                this.Series[0].Name = this.SerieNames[0];
                this.Series[1] = upperLimit;
                this.Series[1].Name = this.SerieNames[1];
                this.Series[2] = lowerLimit;
                this.Series[2].Name = this.SerieNames[2];
                this.Series[3] = signalSerie;
                this.Series[3].Name = this.SerieNames[3];

                if (indicator.DisplayTarget == IndicatorDisplayTarget.RangedIndicator && indicator is IRange)
                {
                    IRange range = (IRange)indicator;
                    indicatorToDecorate = indicatorToDecorate.Sub((range.Max + range.Min) / 2.0f);
                }
                FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
                FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

                int lastExhaustionSellIndex = int.MinValue;
                int lastExhaustionBuyIndex = int.MinValue;
                float exhaustionBuyPrice = highSerie[0];
                float exhaustionSellPrice = lowSerie[0];

                float previousValue = indicatorToDecorate[0];
                float currentValue;
                int i = 0;
                for (i = 1; i < indicatorToDecorate.Count - 1; i++)
                {
                    if (indicatorToDecorate[i] > 0)
                    {
                        this.Events[PositiveIndex][i] = true;
                        countPositive++;
                        countNegative = 0;
                    }
                    else
                    {
                        this.Events[NegativeIndex][i] = true;
                        countPositive = 0;
                        countNegative++;
                    }
                    if (indicatorToDecorate[i] > signalSerie[i])
                    {
                        this.Events[BullishIndex][i] = true;
                    }
                    else
                    {
                        this.Events[BearishIndex][i] = true;
                    }
                    currentValue = indicatorToDecorate[i];
                    if (currentValue == previousValue)
                    {
                        if (indicatorToDecorate.IsBottomIsh(i))
                        {
                            if (currentValue <= oversold)
                            {
                                // This is an exhaustion selling
                                this.Events[ExhaustionBottomIndex][i + 1] = true;
                                exhaustionSellPrice = lowSerie[i];
                                lastExhaustionSellIndex = i + 1;
                            }
                            else
                            {
                                // Check if divergence
                                if (lowSerie[i] <= exhaustionSellPrice)
                                {
                                    this.Events[BullishDivergenceIndex][i + 1] = true;
                                }
                            }
                        }
                        else if (indicatorToDecorate.IsTopIsh(i))
                        {
                            if (currentValue >= overbought)
                            {
                                // This is an exhaustion buying
                                this.Events[ExhaustionTopIndex][i + 1] = true;
                                exhaustionBuyPrice = highSerie[i];
                                lastExhaustionBuyIndex = i + 1;
                            }
                            else
                            {
                                // Check if divergence
                                if (highSerie[i] >= exhaustionBuyPrice)
                                {
                                    this.Events[BearishDivergenceIndex][i + 1] = true;
                                }
                            }
                        }
                    }
                    else if (currentValue < previousValue)
                    {
                        if (indicatorToDecorate.IsBottom(i))
                        {
                            if (currentValue <= oversold)
                            {
                                // This is an exhaustion selling
                                this.Events[ExhaustionBottomIndex][i + 1] = true;
                                exhaustionSellPrice = lowSerie[i];
                                lastExhaustionSellIndex = i + 1;
                            }
                            else
                            {
                                // Check if divergence
                                if (lowSerie[i] <= exhaustionSellPrice)
                                {
                                    this.Events[BullishDivergenceIndex][i + 1] = true;
                                }
                            }
                        }
                    }
                    else if (currentValue > previousValue)
                    {
                        if (indicatorToDecorate.IsTop(i))
                        {
                            if (currentValue >= overbought)
                            {
                                // This is an exhaustion selling
                                this.Events[ExhaustionTopIndex][i + 1] = true;
                                exhaustionBuyPrice = highSerie[i];
                                lastExhaustionBuyIndex = i + 1;
                            }
                            else
                            {
                                // Check if divergence
                                if (highSerie[i] >= exhaustionBuyPrice)
                                {
                                    this.Events[BearishDivergenceIndex][i + 1] = true;
                                }
                            }
                        }
                    }
                    previousValue = currentValue;

                    // Exhaustion occured events
                    if (lookbackPeriod > 0)
                    {
                        if (i + 1 - lookbackPeriod < lastExhaustionBuyIndex)
                        {
                            this.Events[ExhaustionTopOccuredIndex][i + 1] = true;
                        }
                        if (i + 1 - lookbackPeriod < lastExhaustionSellIndex)
                        {
                            this.Events[ExhaustionBottomOccuredIndex][i + 1] = true;
                        }
                    }

                    this.Events[BearFailureIndex][i] = (this.Events[ExhaustionTopOccuredIndex][i] && countPositive == 1);
                    this.Events[BullFailureIndex][i] = (this.Events[ExhaustionBottomOccuredIndex][i] && countNegative == 1);
                }
            }
            else
            {
                for (int i = 0; i < this.EventNames.Length; i++)
                {
                    this.Events[i] = new BoolSerie(0, this.EventNames[i]);
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
                        new Pen(Color.Green), new Pen(Color.Red),
                        new Pen(Color.Transparent), new Pen(Color.Transparent),
                        new Pen(Color.Transparent), new Pen(Color.Transparent),
                        new Pen(Color.Transparent), new Pen(Color.Transparent),
                        new Pen(Color.Transparent), new Pen(Color.Transparent) };
                    eventPens[0].Width = 3;
                    eventPens[1].Width = 3;
                    eventPens[2].Width = 2;
                    eventPens[3].Width = 2;
                    eventPens[4].Width = 2;
                }
                return eventPens;
            }
        }

        private static readonly string[] eventNames = new string[]
        {
            "ExhaustionTop", "ExhaustionBottom",
            "BearishDivergence", "BullishDivergence",
            "BearFailure", "BullFailure",
            "ExhaustionTopOccured", "ExhaustionBottomOccured",
            "Positive", "Negative",
            "Bullish", "Bearish"
        };

        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[]
        {
            true, true,
            true, true,
            true, true,
            false, false,
            false, false,
            false, false
        };
        public override bool[] IsEvent => isEvent;
    }
}
