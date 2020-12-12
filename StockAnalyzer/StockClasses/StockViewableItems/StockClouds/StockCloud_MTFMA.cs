using StockAnalyzer.StockMath;
using System;
using System.Drawing;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_MTFMA : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget
        {
            get { return IndicatorDisplayTarget.PriceIndicator; }
        }
        public override string[] ParameterNames
        {
            get { return new string[] { "FastBarCount", "SlowBarCount" }; }
        }

        public override Object[] ParameterDefaultValues
        {
            get { return new Object[] { 3, 9 }; }
        }
        public override ParamRange[] ParameterRanges
        {
            get { return new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) }; }
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
        public override string[] SerieNames { get { return new string[] { "Bull", "Bear", "Signal" }; } }
        public override void ApplyTo(StockSerie stockSerie)
        {
            int fastBarCount = (int)this.parameters[0];
            int slowBarCount = (int)this.parameters[1];

            var closeSerie = stockSerie.GetSerie(StockDataType.CLOSE);

            var fastSerie = new FloatSerie(stockSerie.Count);
            var slowSerie = new FloatSerie(stockSerie.Count);
            fastSerie[0] = fastSerie[1] = fastSerie[2] = closeSerie[2];
            slowSerie[0] = slowSerie[1] = slowSerie[2] = closeSerie[2];

            float fast0;
            float fast1 = closeSerie[2];
            float fast2 = closeSerie[2];
            float slow0;
            float slow1 = closeSerie[2];
            float slow2 = closeSerie[2];
            var previousValue = stockSerie.Values.ElementAt(2);

            int i = 3;
            int fastCount = 3;
            int slowCount = 3;
            foreach (var dailyValue in stockSerie.Values.Skip(3))
            {
                if (fastCount >= fastBarCount)
                { // New week starting
                    fast0 = fast1;
                    fast1 = fast2;
                    fast2 = previousValue.CLOSE;
                    fastSerie[i] = (fast0 + fast1 + fast2) / 3;
                    fastCount = 0;
                }
                else
                {
                    fastSerie[i] = fastSerie[i - 1];
                    fastCount++;
                }
                if (slowCount >= slowBarCount)
                { // New month starting
                    slow0 = slow1;
                    slow1 = slow2;
                    slow2 = previousValue.CLOSE;
                    slowSerie[i] = (slow0 + slow1 + slow2) / 3;
                    slowCount = 0;
                }
                else
                {
                    slowSerie[i] = slowSerie[i - 1];
                    slowCount++;
                }

                i++;
                previousValue = dailyValue;
            }

            this.Series[0] = fastSerie;
            this.Series[1] = slowSerie;
            this.Series[2] = closeSerie.CalculateMA(3);

            // Detecting events
            base.GenerateEvents(stockSerie);
            for (i = 5; i < stockSerie.Count; i++)
            {
                var bullVal = fastSerie[i];
                var bearVal = slowSerie[i];
                var signalVal = this.Series[2][i];
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
