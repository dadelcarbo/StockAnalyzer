using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TRENDBODYAgent : StockAgentBase
    {
        public TRENDBODYAgent()
        {
            Period = 50;
        }

        [StockAgentParam(5, 80, 5)]
        public int Period { get; set; }

        public override string Description => "Buy with TRENDBODY Bull Events";


        public override string DisplayIndicator => $"CLOUD|TRENDBODY({Period})";

        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            var cloud = stockSerie.GetCloud($"TRENDBODY({Period})");
            bullEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "CloudUp")];
            bearEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "CloudDown")];
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
            if (bearEvents[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
