using System;
using System.Drawing;
using System.Linq;
using StockAnalyzer.StockMath;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_OVERBOUGHTSR : StockUpDownIndicatorBase
    {
        public StockIndicator_OVERBOUGHTSR()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override IndicatorDisplayStyle DisplayStyle
        {
            get { return IndicatorDisplayStyle.SupportResistance; }
        }

        public override string[] ParameterNames
        {
            get { return new string[] { "Indicator", "Overbought", "Oversold" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { "STOKS(30_3_3)", 75f, 25f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeIndicator(), new ParamRangeFloat(-100f, 100f), new ParamRangeFloat(-100f, 100f) }; }
        }

        public override string[] SerieNames { get { return new string[] { "OVERBOUGHT.S", "OVERBOUGHT.R" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie supportSerie, resistanceSerie, secondarySupport, secondaryResistance;
            IStockIndicator stokIndicator = stockSerie.GetIndicator(this.Parameters[0].ToString().Replace("_", ","));

            stockSerie.CalculateOverboughtSR(stokIndicator.Series[0], (float)this.Parameters[1], (float)this.Parameters[2], out supportSerie, out resistanceSerie);
            this.Series[0] = supportSerie;
            this.Series[1] = resistanceSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            BoolSerie supportDetectedSerie = this.Events[0];
            BoolSerie resistanceDetectedSerie = this.Events[1];
            BoolSerie pullbackDetectedSerie = this.Events[2];
            BoolSerie endOfTrendDetectedSerie = this.Events[3];
            BoolSerie higherLowDetectedSerie = this.Events[4];
            BoolSerie lowerHighDetectedSerie = this.Events[5];
            BoolSerie resistanceBrokenSerie = this.Events[6];
            BoolSerie supportBrokenSerie = this.Events[7];
            BoolSerie bullishSerie = this.Events[8];
            BoolSerie bearishSerie = this.Events[9];
            BoolSerie higherHighDetectedSerie = this.Events[10];
            BoolSerie lowerLowDetectedSerie = this.Events[11];
            int i;

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            float previousSupport = closeSerie[0];
            float previousResistance = closeSerie[0];
            float brokenSupport = float.NaN;
            float brokenResistance = float.NaN;
            bool waitForEndOfUpTrend = false;
            bool waitForEndOfDownTrend = false;

            bool isBullish = false;
            bool isBearish = false;

            for (i = 1; i < stockSerie.Count; i++)
            {
                if (float.IsNaN(brokenSupport))
                {
                    if (closeSerie[i] < previousSupport)
                    {
                        brokenSupport = previousSupport;
                        supportBrokenSerie[i] = true;
                        isBullish = false;
                        isBearish = true;
                    }
                }
                if (float.IsNaN(brokenResistance))
                {
                    if (closeSerie[i] > previousResistance)
                    {
                        brokenResistance = previousResistance;
                        resistanceBrokenSerie[i] = true;
                        isBullish = true;
                        isBearish = false;
                    }
                }
                if (!float.IsNaN(supportSerie[i])) // Support exists
                {
                    if (float.IsNaN(supportSerie[i - 1]))
                    {
                        supportDetectedSerie[i] = true;
                        if (supportSerie[i] > previousSupport)
                        {
                            higherLowDetectedSerie[i] = true;
                            if (supportSerie[i] > brokenResistance)
                            {
                                pullbackDetectedSerie[i] = true;
                                waitForEndOfUpTrend = true;
                            }
                        }
                        else if (supportSerie[i] < previousSupport)
                        {
                            lowerLowDetectedSerie[i] = true;
                        }
                        if (waitForEndOfDownTrend)
                        {
                            endOfTrendDetectedSerie[i] = true;
                            waitForEndOfDownTrend = false;
                        }
                        brokenResistance = float.NaN;
                    }
                    else
                    {
                        if (supportSerie[i] > previousSupport)
                        {
                            higherLowDetectedSerie[i] = true;
                        }
                        else if (supportSerie[i] < previousSupport)
                        {
                            lowerLowDetectedSerie[i] = true;
                        }
                    }
                    previousSupport = supportSerie[i];
                }

                if (!float.IsNaN(resistanceSerie[i])) // Resistance exists
                {
                    if (float.IsNaN(resistanceSerie[i - 1]))
                    {
                        resistanceDetectedSerie[i] = true;
                        if (resistanceSerie[i] < previousResistance)
                        {
                            lowerHighDetectedSerie[i] = true;
                            if (resistanceSerie[i] < brokenSupport)
                            {
                                pullbackDetectedSerie[i] = true;
                                waitForEndOfDownTrend = true;
                            }
                        }
                        else if (resistanceSerie[i] > previousResistance)
                        {
                            higherHighDetectedSerie[i] = true;
                        }
                        if (waitForEndOfUpTrend)
                        {
                            endOfTrendDetectedSerie[i] = true;
                            waitForEndOfUpTrend = false;
                        }
                        brokenSupport = float.NaN;
                    }
                    else
                    {
                        if (resistanceSerie[i] < previousResistance)
                        {
                            lowerHighDetectedSerie[i] = true;
                        }
                        else if (resistanceSerie[i] > previousResistance)
                        {
                            higherHighDetectedSerie[i] = true;
                        }
                    }
                    previousResistance = resistanceSerie[i];

                }
                bullishSerie[i] = isBullish;
                bearishSerie[i] = isBearish;
            }
        }

        static string[] eventNames = new string[] {
            "SupportDetected", "ResistanceDetected",
            "Pullback", "EndOfTrend",
            "HigherLow", "LowerHigh",
            "ResistanceBroken", "SupportBroken",
            "Bullish", "Bearish",
            "HigherHigh", "LowerLow"
        };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { 
            true, true, 
            true, true, 
            true, true, 
            true, true, 
            false, false, 
            true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
