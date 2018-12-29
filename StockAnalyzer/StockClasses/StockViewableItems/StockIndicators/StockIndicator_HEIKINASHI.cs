using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HEIKINASHI : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { }; }
        }
        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { }; }
        }

        public override string[] SerieNames { get { return new string[] { "Open", "High", "Low", "Close" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkRed), new Pen(Color.Black), new Pen(Color.Black), new Pen(Color.DarkGreen) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate Donchian Channel
            FloatSerie haOpen = new FloatSerie(stockSerie.Count);
            FloatSerie haHigh = new FloatSerie(stockSerie.Count);
            FloatSerie haLow = new FloatSerie(stockSerie.Count);
            FloatSerie haClose = new FloatSerie(stockSerie.Count);

            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            haOpen[0] = openSerie[0];
            haHigh[0] = highSerie[0];
            haLow[0] = lowSerie[0];
            haClose[0] = closeSerie[0];

            float previousOpen = openSerie[0];
            float previousClose = closeSerie[0];

            for (int i = 1; i < stockSerie.Count; i++)
            {
                float dailyHigh = highSerie[i];
                float dailyLow = lowSerie[i];
                float open = (previousOpen + previousClose) / 2f; // (HA-Open(-1) + HA-Close(-1)) / 2 
                float high = Math.Max(Math.Max(dailyHigh, previousOpen), previousClose); // Maximum of the High(0), HA-Open(0) or HA-Close(0) 
                float low = Math.Min(Math.Min(dailyLow, previousOpen), previousClose); // Minimum of the Low(0), HA-Open(0) or HA-Close(0) 
                float close = (openSerie[i] + dailyHigh + dailyLow + closeSerie[i]) / 4f; // (Open(0) + High(0) + Low(0) + Close(0)) / 4

                haOpen[i] = open;
                haHigh[i] = high;
                haLow[i] = low;
                haClose[i] = close;

                previousClose = close;
                previousOpen = open;

            }

            int count = 0;
            this.series[count] = haOpen;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = haHigh;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = haLow;
            this.Series[count].Name = this.SerieNames[count];

            this.series[++count] = haClose;
            this.Series[count].Name = this.SerieNames[count];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            bool upTrend = true;

            //for (int i = period; i < stockSerie.Count; i++)
            //{
            //    count = 0;

            //    if (upTrend)
            //    {
            //        upTrend = midLine[i] >= midLine[i - 1];
            //    }
            //    else
            //    {
            //        upTrend = midLine[i] > midLine[i - 1];
            //    }

            //    this.Events[count++][i] = upTrend;
            //    this.Events[count++][i] = !upTrend;
            //    this.Events[count++][i] = (!this.Events[0][i - 1]) && (this.Events[0][i]);
            //    this.Events[count++][i] = (this.Events[0][i - 1]) && (!this.Events[0][i]);

            //    this.Events[count++][i] = closeSerie[i] > upLine[i];
            //    this.Events[count++][i] = closeSerie[i] > midUpLine[i];
            //    this.Events[count++][i] = closeSerie[i] > midLine[i];
            //    this.Events[count++][i] = closeSerie[i] < midLine[i];
            //    this.Events[count++][i] = closeSerie[i] < midDownLine[i];
            //    this.Events[count++][i] = closeSerie[i] < downLine[i];

            //    this.Events[count++][i] = highSerie[i] > upLine[i];
            //    this.Events[count++][i] = highSerie[i] > midUpLine[i];
            //    this.Events[count++][i] = highSerie[i] > midLine[i];
            //    this.Events[count++][i] = highSerie[i] < midLine[i];
            //    this.Events[count++][i] = highSerie[i] < midDownLine[i];
            //    this.Events[count++][i] = highSerie[i] < downLine[i];

            //    this.Events[count++][i] = lowSerie[i] > upLine[i];
            //    this.Events[count++][i] = lowSerie[i] > midUpLine[i];
            //    this.Events[count++][i] = lowSerie[i] > midLine[i];
            //    this.Events[count++][i] = lowSerie[i] < midLine[i];
            //    this.Events[count++][i] = lowSerie[i] < midDownLine[i];
            //    this.Events[count++][i] = lowSerie[i] < downLine[i];

            //    this.Events[count++][i] = lowSerie[i - 1] <= midUpLine[i - 1] && lowSerie[i] > midUpLine[i];
            //    this.Events[count++][i] = lowSerie[i - 1] <= midLine[i - 1] && lowSerie[i] > midLine[i];
            //    this.Events[count++][i] = lowSerie[i - 1] <= midDownLine[i - 1] && lowSerie[i] > midDownLine[i];
            //    this.Events[count++][i] = lowSerie[i - 1] <= downLine[i - 1] && lowSerie[i] > downLine[i];

            //    this.Events[count++][i] = highSerie[i - 1] >= upLine[i - 1] && highSerie[i] < upLine[i];
            //    this.Events[count++][i] = highSerie[i - 1] >= midUpLine[i - 1] && highSerie[i] < midUpLine[i];
            //    this.Events[count++][i] = highSerie[i - 1] >= midLine[i - 1] && highSerie[i] < midLine[i];
            //    this.Events[count++][i] = highSerie[i - 1] >= midDownLine[i - 1] && highSerie[i] < midDownLine[i];
            //}
        }

        static string[] eventNames = new string[]
          {
            "Uptrend", "DownTrend", "BrokenUp","BrokenDown",
            "CloseAboveUpLine", "CloseAboveMidUpLine", "CloseAboveMidLine", "CloseBelowMidLine", "CloseBelowMidLowLine", "CloseBelowLowLine",
            "HighAboveUpLine", "HighAboveMidUpLine", "HighAboveMidLine", "HighBelowMidLine", "HighBelowMidLowLine", "HighBelowLowLine",
            "LowAboveUpLine", "LowAboveMidUpLine", "LowAboveMidLine", "LowBelowMidLine", "LowBelowMidLowLine", "LowBelowLowLine",
            "TouchedDownMidUpLine", "TouchedDownMidLine", "TouchedDownMidLowLine", "TouchedDownLowLine",
            "TouchedUpUpLine",   "TouchedUpMidUpLine", "TouchedUpMidLine", "TouchedUpMidLowLine"
          };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[]
          {
            false, false, true, true,
            false, false, false, false, false, false,
            false, false, false, false, false, false,
            false, false, false, false, false, false,
            true, true, true, true,
            true, true, true, true
          };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
