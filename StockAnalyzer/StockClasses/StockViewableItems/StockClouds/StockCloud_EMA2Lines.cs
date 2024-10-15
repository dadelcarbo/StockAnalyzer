using StockAnalyzer.StockMath;
using System;
using System.Drawing;

namespace StockAnalyzer.StockClasses.StockViewableItems.StockClouds
{
    public class StockCloud_EMA2Lines : StockCloudBase
    {
        public override IndicatorDisplayTarget DisplayTarget => IndicatorDisplayTarget.PriceIndicator;
        public override string Definition => "Paint a cloud base on two EMA lines";
        public override string[] ParameterNames => new string[] { "FastPeriod", "SlowPeriod", "SignalPeriod" };

        public override Object[] ParameterDefaultValues => new Object[] { 20, 50, 3 };
        public override ParamRange[] ParameterRanges => new ParamRange[] { new ParamRangeInt(1, 500), new ParamRangeInt(1, 500), new ParamRangeInt(1, 500) };

        public override string[] SerieNames => new string[] { "Bull", "Bear", "Signal" };
        public override void ApplyTo(StockSerie stockSerie)
        {
            FloatSerie bullSerie = stockSerie.GetIndicator($"EMA({(int)this.parameters[0]})").Series[0];
            FloatSerie bearSerie = stockSerie.GetIndicator($"EMA({(int)this.parameters[1]})").Series[0];

            var signalName = $"EMA({(int)this.parameters[2]})";
            FloatSerie signalSerie = stockSerie.GetIndicator(signalName).Series[0];

            this.Series[0] = bullSerie;
            this.Series[1] = bearSerie;
            this.Series[2] = signalSerie;
            this.Series[2].Name = $"EMA({(int)this.parameters[2]})";

            // Detecting events
            base.GenerateEvents(stockSerie);
            for (int i = 5; i < stockSerie.Count; i++)
            {
                var bullVal = bullSerie[i];
                var bearVal = bearSerie[i];
                var signalVal = signalSerie[i];
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
