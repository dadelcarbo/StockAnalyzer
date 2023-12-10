using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_HIGHLOWBARS : StockIndicatorBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override object[] ParameterDefaultValues
        {
            get { return new Object[] { 50, 25 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "TrendPeriod", "BreakPeriod" }; }
        }

        public override string[] SerieNames { get { return new string[] { }; } }

        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { };
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            int trendPeriod = (int)this.parameters[0];
            int breakPeriod = (int)this.parameters[1];

            // Detecting events
            bool bull = false, bear = false;

            // "BullStart", "BearStart", "BullEnd", "BearEnd", "Bull", "Bear"
            for (int i = Math.Max(trendPeriod, breakPeriod); i < closeSerie.Count; i++)
            {
                if (bull)
                {
                    var lowest = lowSerie.GetMin(i - breakPeriod, i - 1);
                    if (closeSerie[i] < lowest)
                    {
                        bull = false;
                        this.eventSeries[2][i] = true; // BullEnd
                    }
                    else
                    {
                        this.eventSeries[4][i] = true; // Bull
                    }
                }
                else
                {
                    var highest = highSerie.GetMax(i - trendPeriod, i - 1);
                    if (closeSerie[i] > highest)
                    {
                        bull = true;
                        this.eventSeries[0][i] = true; // BullStart
                        this.eventSeries[4][i] = true; // Bull
                    }
                }
                if (bear)
                {
                    var highest = highSerie.GetMax(i - breakPeriod, i - 1);
                    if (closeSerie[i] > highest)
                    {
                        bear = false;
                        this.eventSeries[3][i] = true; // BearEnd
                    }
                    else
                    {
                        this.eventSeries[5][i] = true; // Bear
                    }
                }
                else
                {
                    var lowest = lowSerie.GetMin(i - trendPeriod, i - 1);
                    if (closeSerie[i] < lowest)
                    {
                        bear = true;
                        this.eventSeries[1][i] = true; // BearStart
                        this.eventSeries[5][i] = true; // Bear
                    }
                }
            }
        }

        static string[] eventNames = new string[] { "BullStart", "BearStart", "BullEnd", "BearEnd", "Bull", "Bear" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}


