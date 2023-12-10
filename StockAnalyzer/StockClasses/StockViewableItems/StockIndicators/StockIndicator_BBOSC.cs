using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_BBOSC : StockIndicatorBase
    {
        public StockIndicator_BBOSC()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "FastPeriod", "SlowPeriod", "NbUpDev", "NbDownDev", "MAType" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 50, 2.0f, -2.0f, "MA" }; }
        }
        static List<string> emaTypes = new List<string>() { "EMA", "MA", "EA" };
        public override ParamRange[] ParameterRanges
        {
            get
            {
                return new ParamRange[]
            {
                new ParamRangeInt(1, 500),
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-5.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 5.0f),
                new ParamRangeMA()
            };
            }
        }

        public override string[] SerieNames { get { return new string[] { "BBOSC", this.parameters[4] + "(" + (int)this.parameters[1] + ")", this.parameters[4] + "(" + (int)this.parameters[0] + ")" }; } }

        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.DarkGreen), new Pen(Color.DarkRed), new Pen(Color.Blue) };
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
            FloatSerie bbOSC = new FloatSerie(stockSerie.Count);
            FloatSerie emaSlowSerie = stockSerie.GetIndicator(this.parameters[4] + "(" + (int)this.parameters[1] + ")").Series[0];
            FloatSerie emaFastSerie = stockSerie.GetIndicator(this.parameters[4] + "(" + (int)this.parameters[0] + ")").Series[0];
            FloatSerie oscSerie = (emaFastSerie - emaSlowSerie) / emaFastSerie;

            bbOSC[0] = emaFastSerie[0];
            for (int i = 1; i < stockSerie.Count; i++)
            {
                if (oscSerie[i] >= 0)
                {
                    this.eventSeries[2][i] = lowSerie[i] > emaSlowSerie[i];
                    bbOSC[i] = emaFastSerie[i] * (oscSerie[i] * (float)this.parameters[2] + 1f);
                    this.eventSeries[1][i] = highSerie[i - 1] > bbOSC[i - 1] && highSerie[i] < bbOSC[i];
                }
                else
                {
                    this.eventSeries[3][i] = highSerie[i] < emaSlowSerie[i];
                    bbOSC[i] = emaFastSerie[i] / (oscSerie[i] * -(float)this.parameters[2] + 1f);
                    this.eventSeries[0][i] = lowSerie[i - 1] < bbOSC[i - 1] && lowSerie[i] > bbOSC[i];
                }
            }

            this.series[0] = bbOSC;
            this.Series[0].Name = this.SerieNames[0];

            this.series[1] = emaSlowSerie;
            this.Series[1].Name = this.SerieNames[1];

            this.series[2] = emaFastSerie;
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
