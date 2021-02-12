using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class CupHandleAgent : StockAgentBase
    {
        public CupHandleAgent()
        {
            Period = 13;
        }

        [StockAgentParam(2, 80)]
        public int Period { get; set; }

        public override string Description => "Buy with Cup and Handle signal";
        public override string DisplayIndicator => $"AUTODRAWING|CUPHANDLE({Period})";


        IStockEvent eventSerie;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            eventSerie = stockSerie.GetAutoDrawing($"CUPHANDLE({Period})");
            bullEvents = eventSerie.Events[Array.IndexOf<string>(eventSerie.EventNames, "BrokenUp")];
            bearEvents = eventSerie.Events[Array.IndexOf<string>(eventSerie.EventNames, "BrokenDown")];
            return bullEvents != null && bearEvents != null;
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
