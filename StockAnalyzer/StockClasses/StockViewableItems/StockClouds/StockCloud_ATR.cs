using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_ATR : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string Definition => "Paint a cloud based on ATR Band (Keltner Band)";

        public override string[] ParameterNames
        {
            get { return new string[] { "Period", "ATRPeriod", "NbUpDev", "NbDownDev", "MAType" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 20, 10, 2.0f, -2.0f, "EMA" }; }
        }
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
        public override Pen[] SeriePens
        {
            get
            {
                if (seriePens == null)
                {
                    seriePens = new Pen[] { new Pen(Color.Green, 1), new Pen(Color.DarkRed, 1), new Pen(Color.DarkBlue, 2) };
                }
                return seriePens;
            }
        }
        public override string[] SerieNames { get { return new string[] { "Bull", "Bear", "MA" }; } }
        public override void ApplyTo(StockSerie stockSerie)
        {
            var atrBandIndicator = stockSerie.GetIndicator($"ATRBAND({(int)this.parameters[0]},{(int)this.parameters[1]},{(float)this.parameters[2]},{(float)this.parameters[3]},{this.parameters[4]})");
            var bandUp = atrBandIndicator.Series[0];
            var bandDown = atrBandIndicator.Series[1];
            var maSerie = atrBandIndicator.Series[2];

            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);
            FloatSerie openSerie = stockSerie.GetSerie(StockDataType.OPEN);

            var bullSerie = new FloatSerie(stockSerie.Count);
            var bearSerie = new FloatSerie(stockSerie.Count);

            bool isBull = true;
            for (int i = 0; i < stockSerie.Count; i++)
            {
                if (isBull)
                    isBull = closeSerie[i] > bandDown[i];
                else
                    isBull = closeSerie[i] > bandUp[i];

                if (isBull)
                {
                    bullSerie[i] = bandUp[i];
                    bearSerie[i] = bandDown[i];
                }
                else
                {
                    bullSerie[i] = bandDown[i];
                    bearSerie[i] = bandUp[i];
                }
            }

            this.Series[0] = bullSerie;
            this.Series[1] = bearSerie;
            this.Series[2] = maSerie;
            this.Series[2].Name = maSerie.Name;

            // Detecting events
            base.GenerateEvents(stockSerie);
            for (int i = 5; i < stockSerie.Count; i++)
            {
                var bullVal = bullSerie[i];
                var bearVal = bearSerie[i];
                var signalVal = maSerie[i];
                var upBand = Math.Max(bullVal, bearVal);
                var lowBand = Math.Min(bullVal, bearVal);

                if (signalVal > upBand)
                {
                    this.Events[7][i] = true;
                    this.Events[10][i] = !this.Events[7][i - 1];
                }
                else if (signalVal < lowBand)
                {
                    this.Events[8][i] = true;
                    this.Events[11][i] = !this.Events[8][i - 1];
                }
                else
                {
                    this.Events[9][i] = true;
                }
                this.Events[12][i] = (closeSerie[i - 1] < upBand || openSerie[i] < upBand) && closeSerie[i] > upBand;
                this.Events[13][i] = (closeSerie[i - 1] > lowBand || openSerie[i] > lowBand) && closeSerie[i] < lowBand;
            }
        }
        public override string[] EventNames => eventNames;

        protected static new string[] eventNames = new string[]
  {
             "AboveCloud", "BelowCloud", "InCloud",     // 0,1,2
             "BullishCloud", "BearishCloud",            // 3, 4
             "CloudUp", "CloudDown",                    // 5, 6
             "SignalUp", "SignalDown" , "SignalInCloud",// 7,8,9
             "BrokenUp", "BrokenDown",                  // 10,11
             "CloseAbove", "CloseBelow"                 // 12,13
  };
        public override bool[] IsEvent => isEvent;

        protected static new readonly bool[] isEvent = new bool[] { false, false, false, false, false, true, true, false, false, false, true, true, true, true };

    }
}
