using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class MyOSCAgent : StockAgentBase
    {
        public MyOSCAgent()
        {
            Period = 13;
            SignalPeriod = 6;
        }

        [StockAgentParam(10, 120)]
        public int Period { get; set; }

        public int SignalPeriod { get; set; }

        public override string Description => "Buy when MyOSC crosses above zero line";

        FloatSerie osc;
        protected override void Init(StockSerie stockSerie)
        {
            osc = stockSerie.GetIndicator($"MYOSC({Period},{SignalPeriod})").Series[0];
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (osc[index - 1] < 0 && osc[index] >= 0) // bar fast above slow EMA
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (osc[index] < 0) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
