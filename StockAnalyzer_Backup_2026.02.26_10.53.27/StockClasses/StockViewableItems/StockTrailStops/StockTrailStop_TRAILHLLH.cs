using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops
{
    public class StockTrailStop_TRAILHLLH : StockTrailStopBase
    {
        public StockTrailStop_TRAILHLLH()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override bool RequiresVolumeData { get { return false; } }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "InputSmoothing" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 12, 1 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override string[] SerieNames { get { return new string[] { "TRAILEMA.LS", "TRAILEMA.SS" }; } }

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
            FloatSerie longStopSerie;
            FloatSerie shortStopSerie;
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            stockSerie.CalculateEMATrailStop((int)this.Parameters[0], (int)this.Parameters[1], out longStopSerie, out shortStopSerie);
            this.Series[0] = longStopSerie;
            this.Series[1] = shortStopSerie;

            // Generate events
            this.GenerateEvents(stockSerie, longStopSerie, shortStopSerie);
        }
        protected override void GenerateEvents(StockSerie stockSerie, FloatSerie longStopSerie, FloatSerie shortStopSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            float previousHigh = stockSerie.GetSerie(StockDataType.HIGH).GetMax(0, 4);
            float previousLow = stockSerie.GetSerie(StockDataType.LOW).GetMin(0, 4);
            float previousHigh2 = previousHigh;
            float previousLow2 = previousLow;
            bool waitingForEndOfTrend = false;
            bool isBullish = false;
            bool isBearish = false;
            for (int i = 5; i < stockSerie.Count; i++)
            {
                if (!float.IsNaN(longStopSerie[i]) && float.IsNaN(longStopSerie[i - 1]))
                {
                    this.Events[0][i] = true; // SupportDetected

                    if (waitingForEndOfTrend)
                    {
                        this.Events[3][i] = true; // EndOfTrend
                        waitingForEndOfTrend = false;
                    }

                    if (longStopSerie[i] > previousLow)
                    {
                        this.Events[4][i] = true; // HigherLow

                        if (longStopSerie[i] > previousHigh2)
                        {
                            this.Events[2][i] = true; // PB
                            waitingForEndOfTrend = true;
                        }
                    }
                    previousLow2 = previousLow;
                    previousLow = longStopSerie[i];
                }
                if (!float.IsNaN(shortStopSerie[i]) && float.IsNaN(shortStopSerie[i - 1]))
                {
                    this.Events[1][i] = true; // ResistanceDetected

                    if (waitingForEndOfTrend)
                    {
                        this.Events[3][i] = true; // EndOfTrend
                        waitingForEndOfTrend = false;
                    }

                    if (shortStopSerie[i] < previousHigh)
                    {
                        this.Events[5][i] = true; // LowerHigh
                        if (shortStopSerie[i] < previousLow2)
                        {
                            this.Events[2][i] = true; // PB
                            waitingForEndOfTrend = true;
                        }
                    }
                    previousHigh2 = previousHigh;
                    previousHigh = shortStopSerie[i];
                }

                bool supportBroken = float.IsNaN(longStopSerie[i]) && !float.IsNaN(longStopSerie[i - 1]);
                bool resistanceBroken = float.IsNaN(shortStopSerie[i]) && !float.IsNaN(shortStopSerie[i - 1]);

                if (isBullish)
                {
                    isBullish = this.Events[5][i] != true; // LowerHigh
                }
                else
                {
                    isBullish = this.Events[4][i] == true; // HigherLow
                }
                if (isBearish)
                {
                    isBearish = this.Events[4][i] != true; // HigherLow
                }
                else
                {
                    isBearish = this.Events[5][i] == true; // LowerHigh
                }

                this.Events[6][i] = isBullish;
                this.Events[7][i] = isBearish;
            }
        }

    }
}
