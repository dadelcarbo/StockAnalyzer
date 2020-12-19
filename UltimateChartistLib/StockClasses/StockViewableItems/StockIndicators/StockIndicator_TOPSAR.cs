using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TOPSAR : StockIndicatorBase
    {
        public StockIndicator_TOPSAR()
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
            get { return new string[] { "Init", "Step", "Max", "InputSmooting" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 2f, 2f, 20f, 1 }; }
        }

        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeFloat(0.0f, 10.0f), new ParamRangeFloat(0.0f, 10.0f), new ParamRangeFloat(0.01f, 100.0f), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames
        {
            get { return new string[] { "TOPSAR.S", "TOPSAR.R" }; }
        }

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
            float accelerationFactorStart = (float)this.parameters[0]/100f;
            float accelerationFactorStep = (float)this.parameters[1]/100f;
            float accelerationFactorMax = (float)this.parameters[2]/100f;
            int inputSmooting = (int)this.parameters[3];

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            FloatSerie sarSupport;
            FloatSerie sarResistance;

            stockSerie.CalculateTOPSAR(accelerationFactorStep, accelerationFactorStart, accelerationFactorMax, out sarSupport, out sarResistance, inputSmooting);

            this.Series[0] = sarSupport;
            this.Series[1] = sarResistance;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            float previousHigh = stockSerie.Values.First().HIGH, previousLow = stockSerie.Values.First().LOW;
            float previousHigh2 = stockSerie.Values.First().HIGH, previousLow2 = stockSerie.Values.First().LOW;
            bool waitingForEndOfUpTrend = false;
            bool waitingForEndOfDownTrend = false;
            bool isBullish = false;
            bool isBearish = false;
            for (int i = 5; i < stockSerie.Count; i++)
            {
                if (!float.IsNaN(sarSupport[i]) && float.IsNaN(sarSupport[i - 1]))
                {
                    this.Events[0][i] = true; // SupportDetected

                    if (waitingForEndOfDownTrend)
                    {
                        this.Events[3][i] = true; // EndOfTrend
                        waitingForEndOfDownTrend = false;
                    }

                    if (sarSupport[i] > previousLow)
                    {
                        this.Events[4][i] = true; // HigherLow

                        if (sarSupport[i] > previousHigh2)
                        {
                            this.Events[2][i] = true; // PB
                            waitingForEndOfDownTrend = true;
                        }
                    }
                    previousLow2 = previousLow;
                    previousLow = sarSupport[i];
                }
                if (!float.IsNaN(sarResistance[i]) && float.IsNaN(sarResistance[i - 1]))
                {
                    this.Events[1][i] = true; // ResistanceDetected

                    if (waitingForEndOfUpTrend)
                    {
                        this.Events[3][i] = true; // EndOfTrend
                        waitingForEndOfUpTrend = false;
                    }

                    if (sarResistance[i] < previousHigh)
                    {
                        this.Events[5][i] = true; // LowerHigh
                        if (sarResistance[i] < previousLow2)
                        {
                            this.Events[2][i] = true; // PB
                            waitingForEndOfUpTrend = true;
                        }
                    }
                    previousHigh2 = previousHigh;
                    previousHigh = sarResistance[i];
                }

                bool supportBroken = float.IsNaN(sarSupport[i]) && !float.IsNaN(sarSupport[i - 1]);
                this.Events[7][i] = supportBroken;
                bool resistanceBroken = float.IsNaN(sarResistance[i]) && !float.IsNaN(sarResistance[i - 1]);
                this.Events[6][i] = resistanceBroken;

                if (isBullish)
                {
                    isBullish = !supportBroken;
                }
                else
                {
                    isBullish = !float.IsNaN(sarSupport[i]) && float.IsNaN(sarResistance[i]);
                }
                if (isBearish)
                {
                    isBearish = !resistanceBroken;
                }
                else
                {
                    isBearish = float.IsNaN(sarSupport[i]) && !float.IsNaN(sarResistance[i]);
                }

                this.Events[8][i] = isBullish;
                this.Events[9][i] = isBearish;
            }
        }
        private static string[] eventNames = new string[]
      {
         "SupportDetected", "ResistanceDetected", // 0,1
         "Pullback", "EndOfTrend", // 2,3
         "HigherLow", "LowerHigh", // 4,5
         "ResistanceBroken", "SupportBroken", // 6,7
         "Bullish", "Bearish" // 8,9
      };

        //static string[] eventNames = new string[] { "UpTrend", "DownTrend", "BrokenUp", "BrokenDown" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }

        private static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, true, true, false, false };
        //static readonly bool[] isEvent = new bool[] { false, false, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}