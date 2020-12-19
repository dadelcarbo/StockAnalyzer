using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class MyOSC2Agent : StockAgentBase
    {
        public MyOSC2Agent()
        {
            Period = 13;
            SignalPeriod = 6;
        }

        [StockAgentParam(10, 120)]
        public int Period { get; set; }

        [StockAgentParam(2, 20)]
        public int SignalPeriod { get; set; }

        public override string Description => "Buy when MyOSC crosses above zero line";

        FloatSerie osc;
        FloatSerie signal;
        protected override void Init(StockSerie stockSerie)
        {
            osc = stockSerie.GetIndicator($"MYOSC({Period},{SignalPeriod})").Series[0];
            signal = stockSerie.GetIndicator($"MYOSC({Period},{SignalPeriod})").Series[1];
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (osc[index] < 0 && osc[index - 1] < signal[index - 1] && osc[index] >= signal[index]) // bar fast above slow EMA
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (osc[index] < signal[index]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
