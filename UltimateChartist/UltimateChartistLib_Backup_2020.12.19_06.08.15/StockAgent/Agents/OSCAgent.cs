using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class OSCAgent : StockAgentBase
    {
        public OSCAgent()
        {
            SlowPeriod = 30;
            FastPeriod = 5;
        }
        [StockAgentParam(3, 30)]
        public int FastPeriod { get; set; }

        [StockAgentParam(30, 60)]
        public int SlowPeriod { get; set; }

        public override string Description => "Buy with signal on a slow and fast OSC";

        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override void Init(StockSerie stockSerie)
        {
            var osc1 = stockSerie.GetIndicator($"OSC({FastPeriod},{FastPeriod*2},true)");
            var osc2 = stockSerie.GetIndicator($"OSC({SlowPeriod},{SlowPeriod*2},true)");
            bullEvents = osc1.Events[Array.IndexOf<string>(osc1.EventNames, "TurnedPositive")] & osc2.Events[Array.IndexOf<string>(osc2.EventNames, "Bullish")];
            bearEvents = osc2.Events[Array.IndexOf<string>(osc1.EventNames, "Bearish")];
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (bullEvents[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (bearEvents[index]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
