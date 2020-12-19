using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class HIGHLOWBARSAgent : StockAgentBase
    {
        public HIGHLOWBARSAgent()
        {
            Period1 = 50;
            Period2 = 25;
        }

        [StockAgentParam(2, 120)]
        public int Period1 { get; set; }

        [StockAgentParam(2, 120)]
        public int Period2 { get; set; }

        public override string Description => "Buy with HIGHLOWBARS Bull Events";

        IStockEvent highLowBars;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override void Init(StockSerie stockSerie)
        {
            highLowBars = stockSerie.GetIndicator($"HIGHLOWBARS({Period1},{Period2})");
            bullEvents = highLowBars.Events[Array.IndexOf<string>(highLowBars.EventNames, "BullStart")];
            bearEvents = highLowBars.Events[Array.IndexOf<string>(highLowBars.EventNames, "BullEnd")];
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
