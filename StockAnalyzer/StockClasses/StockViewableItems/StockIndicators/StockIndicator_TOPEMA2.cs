using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_TOPEMA2 : StockIndicatorBase
    {
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
            get { return new string[] { "FastPeriod", "SlowPeriod", "InputSmooting" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] {20, 50, 1 }; }
        }

        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }

        public override string[] SerieNames
        {
            get { return new string[] { $"TOPEMA_{parameters[0]}.S", $"TOPEMA_{parameters[0]}.R", $"TOPEMA_{parameters[1]}.S", $"TOPEMA_{parameters[1]}.R" }; }
        }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 1), new Pen(Color.Red, 1), new Pen(Color.Green, 2), new Pen(Color.Red, 2) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            float initGap = 0.0f;
            int fastPeriod = (int)this.parameters[0];
            int slowPeriod = (int)this.parameters[1];
            int inputSmooting = (int)this.parameters[2];

            IStockIndicator fast = stockSerie.GetIndicator($"TOPEMA({initGap},{fastPeriod},{inputSmooting})");
            IStockIndicator slow = stockSerie.GetIndicator($"TOPEMA({initGap},{slowPeriod},{inputSmooting})");

            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);

            FloatSerie sarSupport;
            FloatSerie sarResistance;

            this.Series[0] = fast.Series[0];
            this.Series[0].Name = this.SerieNames[0];
            this.Series[1] = fast.Series[1];
            this.Series[1].Name = this.SerieNames[1];
            this.Series[2] = slow.Series[0];
            this.Series[2].Name = this.SerieNames[2];
            this.Series[3] = slow.Series[1];
            this.Series[3].Name = this.SerieNames[3];

            #region Detecting events
            this.CreateEventSeries(stockSerie.Count);

            float previousHigh = stockSerie.Values.First().HIGH, previousLow = stockSerie.Values.First().LOW;
            float previousHigh2 = stockSerie.Values.First().HIGH, previousLow2 = stockSerie.Values.First().LOW;
            bool waitingForEndOfUpTrend = false;
            bool waitingForEndOfDownTrend = false;
            bool isBullish = false;
            bool isBearish = false;
            //for (int i = 5; i < stockSerie.Count; i++)
            //{
            //    if (!float.IsNaN(sarSupport[i]) && float.IsNaN(sarSupport[i - 1]))
            //    {
            //        this.Events[0][i] = true; // SupportDetected

            //        if (waitingForEndOfDownTrend)
            //        {
            //            this.Events[3][i] = true; // EndOfTrend
            //            waitingForEndOfDownTrend = false;
            //        }

            //        if (sarSupport[i] > previousLow)
            //        {
            //            this.Events[4][i] = true; // HigherLow

            //            if (sarSupport[i] > previousHigh2)
            //            {
            //                this.Events[2][i] = true; // PB
            //                waitingForEndOfDownTrend = true;
            //            }
            //        }
            //        previousLow2 = previousLow;
            //        previousLow = sarSupport[i];
            //    }
            //    if (!float.IsNaN(sarResistance[i]) && float.IsNaN(sarResistance[i - 1]))
            //    {
            //        this.Events[1][i] = true; // ResistanceDetected

            //        if (waitingForEndOfUpTrend)
            //        {
            //            this.Events[3][i] = true; // EndOfTrend
            //            waitingForEndOfUpTrend = false;
            //        }

            //        if (sarResistance[i] < previousHigh)
            //        {
            //            this.Events[5][i] = true; // LowerHigh
            //            if (sarResistance[i] < previousLow2)
            //            {
            //                this.Events[2][i] = true; // PB
            //                waitingForEndOfUpTrend = true;
            //            }
            //        }
            //        previousHigh2 = previousHigh;
            //        previousHigh = sarResistance[i];
            //    }

            //    bool supportBroken = float.IsNaN(sarSupport[i]) && !float.IsNaN(sarSupport[i - 1]);
            //    this.Events[7][i] = supportBroken;
            //    bool resistanceBroken = float.IsNaN(sarResistance[i]) && !float.IsNaN(sarResistance[i - 1]);
            //    this.Events[6][i] = resistanceBroken;

            //    if (isBullish)
            //    {
            //        isBullish = !supportBroken;
            //    }
            //    else
            //    {
            //        isBullish = !float.IsNaN(sarSupport[i]) && float.IsNaN(sarResistance[i]);
            //        this.Events[10][i] = isBullish; // FirstResistanceBroken
            //    }
            //    if (isBearish)
            //    {
            //        isBearish = !resistanceBroken;
            //    }
            //    else
            //    {
            //        isBearish = float.IsNaN(sarSupport[i]) && !float.IsNaN(sarResistance[i]);
            //        this.Events[11][i] = isBearish; // FirstSupportBroken
            //    }

            //    this.Events[8][i] = isBullish;
            //    this.Events[9][i] = isBearish;
            //}
#endregion
        }
        private static string[] eventNames = new string[]
      {
         "SupportDetected", "ResistanceDetected",           // 0,1
         "Pullback", "EndOfTrend",                          // 2,3
         "HigherLow", "LowerHigh",                          // 4,5
         "ResistanceBroken", "SupportBroken",               // 6,7
         "Bullish", "Bearish",                              // 8,9
         "FirstResistanceBroken", "FirstSupportBroken"      // 10,11
      };

        public override string[] EventNames => eventNames;

        private static readonly bool[] isEvent = new bool[] { true, true, true, true, true, true, true, true, false, false, true, true };
        public override bool[] IsEvent => isEvent;
    }
}