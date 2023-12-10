using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_EMA2Lines : StockIndicatorBase
    {
        public StockIndicator_EMA2Lines()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string Name
        {
            get { return "EMA2Lines(" + this.Parameters[0].ToString() + "," + this.Parameters[1].ToString() + ")"; }
        }
        public override string Definition
        {
            get { return "EMA2Lines(int FastPeriod, int SlowPeriod)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "FastPeriod", "SlowPeriod" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 50 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override string[] SerieNames { get { return new string[] { "EMA(" + this.Parameters[0].ToString() + ")", "EMA(" + this.Parameters[1].ToString() + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green, 2), new Pen(Color.DarkRed, 2) };
                return seriePens;
            }
        }
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie fastSerie = stockSerie.GetIndicator(this.SerieNames[0]).Series[0];
            FloatSerie slowSerie = stockSerie.GetIndicator(this.SerieNames[1]).Series[0];

            FloatSerie closeSerie = (stockSerie.GetSerie(StockDataType.CLOSE) + stockSerie.GetSerie(StockDataType.HIGH) + stockSerie.GetSerie(StockDataType.LOW)) / 3.0f;

            this.Series[0] = fastSerie;
            this.Series[1] = slowSerie;

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            for (int i = 2; i < stockSerie.Count; i++)
            {
                if (fastSerie[i] > slowSerie[i])
                {
                    if (closeSerie[i] > fastSerie[i])
                    {
                        this.Events[2][i] = true;
                    }
                    else
                    {
                        this.Events[4][i] = true;
                    }
                    if (fastSerie[i - 1] < slowSerie[i - 1])
                    {
                        this.Events[0][i] = true;
                    }
                }
                else
                {
                    if (closeSerie[i] < fastSerie[i])
                    {
                        this.Events[3][i] = true;
                    }
                    else
                    {
                        this.Events[5][i] = true;
                    }
                    if (fastSerie[i - 1] > slowSerie[i - 1])
                    {
                        this.Events[1][i] = true;
                    }
                }
            }
        }

        static readonly string[] eventNames = new string[] {
          "BullishCrossing", "BearishCrossing",
          "UpTrend", "DownTrend",
          "UpTrendConso", "DownTrendConso" };

        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, false, false, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
