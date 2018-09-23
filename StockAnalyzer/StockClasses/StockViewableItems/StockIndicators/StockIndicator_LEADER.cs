using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_LEADER : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 60, 12 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Lookback", "TrailPeriod" }; }
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
            this.CreateEventSeries(stockSerie.Count);

            int lookback = (int)this.parameters[0];
            var highLowDays = stockSerie.GetIndicator("HIGHLOWDAYS(" + lookback + ")") as IStockEvent;
            int trailPeriod = (int)this.parameters[1];
            var trail = stockSerie.GetTrailStop("TRAILEMA(" + trailPeriod + ",1)") as IStockEvent;

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            // Detecting events
            float lowest = 0f, highest = 0f;

            bool bullish = false;
            bool bearish = false;

            for (int i = lookback; i < closeSerie.Count; i++)
            {
                if (bullish)
                {
                    // Check if uptrend broken
                    if (trail.Events[1][i])
                    {
                        this.eventSeries[4][i] = true; // EndOfBull
                        bullish = false;
                    }
                }
                else if (bearish)
                {
                    // Check if downtrend broken
                    if (trail.Events[0][i])
                    {
                        this.eventSeries[5][i] = true; // EndOfBear
                        bearish = false;
                    }
                }
                else
                {
                    // Check if new trend starting
                    if (highLowDays.Events[0][i])
                    {
                        this.eventSeries[0][i] = bullish = true;
                        bearish = false;
                    }
                    else if (highLowDays.Events[1][i])
                    {
                        this.eventSeries[1][i] = bearish = true;
                        bullish = false;
                    }
                }

                this.eventSeries[2][i] = bullish;
                this.eventSeries[3][i] = bearish;
            }
        }

        static string[] eventNames = new string[] { "NewHigh", "NewLow", "Bullish", "Bearish", "EndOfBull", "EndOfBear" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}


