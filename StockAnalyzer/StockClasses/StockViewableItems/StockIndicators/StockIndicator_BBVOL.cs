using System;
using System.Collections.Generic;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BBVOL : StockIndicatorBase
    {
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
            get { return new Object[] { 20, 2f, -2f, "EMA" }; }
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
                    new ParamRangeMA()
                };
            }
        }

        public override string[] SerieNames { get { return new string[] { "BBUp", "BBDown", this.parameters[3] + "(" + (int)this.parameters[0] + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkRed), new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            // Calculate Bands
            int period = (int)this.parameters[0];
            float upDev = (float)this.parameters[1];
            float downDev = (float)this.parameters[2];

            FloatSerie upperBB = new FloatSerie(stockSerie.Count);
            FloatSerie lowerBB = new FloatSerie(stockSerie.Count);
            FloatSerie emaSerie = stockSerie.GetIndicator(this.parameters[3] + $"({period})").Series[0];

            for (int i = 0; i < period; i++)
            {
                upperBB[i] = emaSerie[i];
                lowerBB[i] = emaSerie[i];
            }
            for (int i = period; i < stockSerie.Count; i++)
            {
                float hst = highSerie.GetMax(i - period, i);
                float lst = lowSerie.GetMin(i - period, i);
                float vol = 0.5f * (hst - lst) / (hst + lst);

                upperBB[i] = emaSerie[i] + upDev * vol * emaSerie[i];
                lowerBB[i] = emaSerie[i] + downDev * vol * emaSerie[i];
            }

            this.series[0] = emaSerie;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = upperBB;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = lowerBB;
            this.Series[2].Name = this.SerieNames[2];
        }

        static string[] eventNames = new string[] { "DownBandOvershot", "UpBandOvershot", "BullRun", "BearRun" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }

        private static readonly bool[] isEvent = new bool[] { true, true, false, false };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
