using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BBEX : StockIndicatorBase
    {
        public StockIndicator_BBEX()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "NbUpDev", "NbDownDev", "MAType" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 2.0f, -2.0f, "MA" }; }
        }
        static List<string> emaTypes = new List<string>() { "EMA", "HMA", "MA", "EA" };
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(0f, 20.0f),
                new ParamRangeFloat(-20.0f, 0.0f),
                new ParamRangeStringList( emaTypes)
                };
            }
        }

        public override string[] SerieNames { get { return new string[] { "BBEXUp", "BBEXDown", this.parameters[3] + "(" + (int)this.parameters[0] + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Blue), new Pen(Color.Blue), new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate Bollinger Bands
            FloatSerie upperBB = null;
            FloatSerie lowerBB = null;
            var emaSerie = stockSerie.GetIndicator(this.parameters[3] + "(" + (int)this.parameters[0] + ")").Series[0];

            stockSerie.GetSerie(StockDataType.CLOSE).CalculateBBEX(emaSerie, (int)this.parameters[0], (float)this.parameters[1], (float)this.parameters[2], ref upperBB, ref lowerBB);

            this.series[0] = upperBB;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = lowerBB;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = emaSerie;
            this.Series[2].Name = this.SerieNames[2];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            //FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            //FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            //bool waitingForBearSignal = false;
            //bool waitingForBullSignal = false;

            // Detecting events
            bool bullish = false;
            bool bearish = false;

            for (int i = (int)this.parameters[0]; i < closeSerie.Count; i++)
            {
                if (bullish)
                {
                    // Check if uptrend broken
                    if (closeSerie[i] < emaSerie[i])
                    {
                        this.eventSeries[4][i] = true; // EndOfBull
                        bullish = false;
                    }
                }
                else if (bearish)
                {
                    // Check if downtrend broken
                    if (closeSerie[i] > emaSerie[i])
                    {
                        this.eventSeries[5][i] = true; // EndOfBear
                        bearish = false;
                    }
                }
                else
                {
                    // Check if new trend starting
                    if (closeSerie[i] > upperBB[i])
                    {
                        this.eventSeries[0][i] = bullish = true;
                        bearish = false;
                    }
                    else if (closeSerie[i] < lowerBB[i])
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
