using System;
using System.Drawing;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockIndicators
{
    public class StockIndicator_KeltnerBand : StockIndicatorBase
    {
        public StockIndicator_KeltnerBand()
        {
        }
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }

        public override string Definition
        {
            get { return "KeltnerBand(int Period, float NbUpDev, float NbDownDev)"; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "Period","NbUpDev","NbDownDev" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] {20, 2.0f, -2.0f }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeFloat(0f, 20.0f), new ParamRangeFloat(-20.0f, 0.0f)}; }
        }

        public override string[] SerieNames { get { return new string[] { "KeltnerBandUp", "KeltnerBandDown" }; } }
        
        public override System.Drawing.Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Blue), new Pen(Color.Blue) };
                }
                return seriePens;
            }
        }

        public override void ApplyTo(StockSerie stockSerie)
        {
            // Calculate Bands
            int period = (int) this.parameters[0];
            FloatSerie ema = stockSerie.GetIndicator( "EMA(" + period + ")").Series[0];
            FloatSerie atr = stockSerie.GetIndicator( "ATR(" + period + ")").Series[0];

            float upCoef = (float) this.parameters[1];
            float downCoef = (float) this.parameters[2];

            FloatSerie upperKeltnerBand = ema + atr * upCoef;
            this.series[0] = upperKeltnerBand;
            this.Series[0].Name = this.SerieNames[0];
            
            FloatSerie lowerKeltnerBand = ema + atr * downCoef; 
            this.series[1] = lowerKeltnerBand;
            this.Series[1].Name = this.SerieNames[1];

            // Detecting events
            this.CreateEventSeries(stockSerie.Count);

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie lowSerie = stockSerie.GetSerie(StockDataType.LOW);
            FloatSerie highSerie = stockSerie.GetSerie(StockDataType.HIGH);

            bool waitingForBearSignal = false;
            bool waitingForBullSignal = false;

            for (int i = 1; i < upperKeltnerBand.Count; i++)
            {
                if (waitingForBearSignal && highSerie[i - 1] >= highSerie[i] && closeSerie[i - 1] >= closeSerie[i])
                {
                    // BearishSignal
                    this.eventSeries[3][i] = true;
                    waitingForBearSignal = false;
                }
                if (highSerie[i] >= upperKeltnerBand[i])
                {
                    waitingForBearSignal = true;
                    this.eventSeries[0][i] = true;
                }
                if (waitingForBullSignal && lowSerie[i - 1] <= lowSerie[i] && closeSerie[i - 1] <= closeSerie[i])
                {
                    // BullishSignal
                    this.eventSeries[2][i] = true;
                    waitingForBullSignal = false;
                }
                if (lowSerie[i] <= lowerKeltnerBand[i])
                {
                    waitingForBullSignal = true;
                    this.eventSeries[1][i] = lowSerie[i] <= lowerKeltnerBand[i];
                }
            }
        }

        static string[] eventNames = new string[] { "UpBandOvershot", "DownBandOvershot", "BullishSignal", "BearishSignal" };
        public override string[] EventNames
        {
            get { return eventNames; }
        }
        static readonly bool[] isEvent = new bool[] { true, true, true, true };
        public override bool[] IsEvent
        {
            get { return isEvent; }
        }
    }
}
