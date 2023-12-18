using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_BB : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string Definition => "Paint a cloud based on the Bollinger bands";

        public override string[] ParameterNames => new string[] { "Period", "NbUpDev", "NbDownDev", "MAType" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 2.0f, -2.0f, "EMA" };
        static List<string> emaTypes = StockIndicatorMovingAvgBase.MaTypes;
        public override ParamRange[] ParameterRanges => new ParamRange[]
                {
                new ParamRangeInt(1, 500),
                new ParamRangeFloat(-20.0f, 20.0f),
                new ParamRangeFloat(-20.0f, 20.0f),
                new ParamRangeMA()
                };
        public override Pen[] SeriePens
        {
            get
            {
                seriePens ??= new Pen[] { new Pen(Color.Green, 1), new Pen(Color.DarkRed, 1), new Pen(Color.DarkBlue, 2) };
                return seriePens;
            }
        }
        public override string[] SerieNames => new string[] { "Bull", "Bear", "MA" };
        public override void ApplyTo(StockSerie stockSerie)
        {
            var bbIndicator = stockSerie.GetIndicator($"BB({(int)this.parameters[0]},{(float)this.parameters[1]},{(float)this.parameters[2]},{this.parameters[3]})");
            var bbUp = bbIndicator.Series[0];
            var bbDown = bbIndicator.Series[1];
            var maSerie = bbIndicator.Series[2];
            FloatSerie closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var bullSerie = new FloatSerie(stockSerie.Count);
            var bearSerie = new FloatSerie(stockSerie.Count);

            bool isBull = true;
            for (int i = 0; i < stockSerie.Count; i++)
            {
                if (isBull)
                    isBull = closeSerie[i] > bbDown[i];
                else
                    isBull = closeSerie[i] > bbUp[i];

                if (isBull)
                {
                    bullSerie[i] = bbUp[i];
                    bearSerie[i] = bbDown[i];
                }
                else
                {
                    bullSerie[i] = bbDown[i];
                    bearSerie[i] = bbUp[i];
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
            }

        }
        public override string[] EventNames => eventNames;

        protected static new string[] eventNames = new string[]
  {
             "AboveCloud", "BelowCloud", "InCloud",     // 0,1,2
             "BullishCloud", "BearishCloud",            // 3, 4
             "CloudUp", "CloudDown",                    // 5, 6
             "SignalUp", "SignalDown" , "SignalInCloud",// 7,8,9
             "BrokenUp", "BrokenDown"                   // 10,11
  };
        public override bool[] IsEvent => isEvent;

        protected static new readonly bool[] isEvent = new bool[] { false, false, false, false, false, true, true, false, false, false, true, true };

    }
}
