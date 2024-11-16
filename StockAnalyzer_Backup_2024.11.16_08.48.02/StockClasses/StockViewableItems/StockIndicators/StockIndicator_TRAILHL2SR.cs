using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TRAILHL2SR : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override IndicatorDisplayStyle DisplayStyle => IndicatorDisplayStyle.SupportResistance;

        public override string Definition => "Trend followinf system initiated on Higher Low which follows on level below";

        public override string[] ParameterNames => new string[] { "Period" };

        public override Object[] ParameterDefaultValues => new Object[] { 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(0, 500) };

        public override string[] SerieNames => new string[] { "TRAILHL2.S", "TRAILHL2.R" };

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            int period = (int)this.Parameters[0];

            var trailStop = stockSerie.GetTrailStop("TRAILHL(" + period + ")");

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);
            {
                var longStopSerie = trailStop.Series[0];
                var supportDetectedSerie = trailStop.Events[0];
                var higherLowSerie = trailStop.Events[4];
                var supportSerie = new FloatSerie(stockSerie.Count, "TRAILHL2.S"); supportSerie.Reset(float.NaN);
                this.Series[0] = supportSerie;


                bool isBull = false;
                float previousLow = closeSerie[0];
                float support = closeSerie[0];
                for (int i = period; i < stockSerie.Count; i++)
                {
                    if (isBull)
                    {
                        if (closeSerie[i] < support) // BullEnd
                        {
                            isBull = false;
                            this.Events[3][i] = true;
                        }
                        else
                        {
                            if (supportDetectedSerie[i])
                            {
                                if (higherLowSerie[i]) // TrailUp
                                {
                                    support = previousLow;
                                }
                                previousLow = longStopSerie[i];
                            }
                            supportSerie[i] = support;
                            this.Events[0][i] = true;
                        }
                    }
                    else
                    {
                        if (supportDetectedSerie[i])
                        {
                            if (higherLowSerie[i]) // Start bull
                            {
                                support = previousLow;
                                isBull = true;
                                supportSerie[i] = support;
                                this.Events[2][i] = true;
                                this.Events[0][i] = true;
                            }
                            previousLow = longStopSerie[i];
                        }
                    }
                }
            }
            {
                var shortStopSerie = trailStop.Series[1];
                var resistanceDetectedSerie = trailStop.Events[1];
                var lowerHighSerie = trailStop.Events[5];
                var resistanceSerie = new FloatSerie(stockSerie.Count, "TRAILHL2.R"); resistanceSerie.Reset(float.NaN);
                this.Series[1] = resistanceSerie;

                bool isBear = false;
                float previousHigh = closeSerie[0];
                float resistance = closeSerie[0];
                for (int i = period; i < stockSerie.Count; i++)
                {
                    if (isBear)
                    {
                        if (closeSerie[i] > resistance) // BearEnd
                        {
                            isBear = false;
                            this.Events[5][i] = true;
                        }
                        else
                        {
                            if (resistanceDetectedSerie[i])
                            {
                                if (lowerHighSerie[i]) // TrailUp
                                {
                                    resistance = previousHigh;
                                }
                                previousHigh = shortStopSerie[i];
                            }
                            resistanceSerie[i] = resistance;
                            this.Events[1][i] = true;
                        }
                    }
                    else
                    {
                        if (resistanceDetectedSerie[i])
                        {
                            if (lowerHighSerie[i]) // Start bear
                            {
                                resistance = previousHigh;
                                isBear = true;
                                resistanceSerie[i] = resistance;
                                this.Events[4][i] = true;
                                this.Events[1][i] = true;
                            }
                            previousHigh = shortStopSerie[i];
                        }
                    }
                }
            }
        }

        static readonly string[] eventNames = new string[] { "Bullish", "Bearish", "BullStart", "BullEnd", "BearStart", "BearEnd" };
        public override string[] EventNames => eventNames;
        static readonly bool[] isEvent = new bool[] { false, false, true, true, false, false };
        public override bool[] IsEvent => isEvent;
    }
}
