using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class HeikinAshiAgent : StockAgentBase
    {
        public HeikinAshiAgent()
        {
        }

        public override string Description => "Buy with HeikinAshi bars";

        IStockEvent eventSerie;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override void Init(StockSerie stockSerie)
        {
            eventSerie = stockSerie.GetIndicator($"HEIKINASHI()");
            bullEvents = eventSerie.Events[Array.IndexOf<string>(eventSerie.EventNames, "BrokenUp")];
            bearEvents = eventSerie.Events[Array.IndexOf<string>(eventSerie.EventNames, "BrokenDown")];
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
