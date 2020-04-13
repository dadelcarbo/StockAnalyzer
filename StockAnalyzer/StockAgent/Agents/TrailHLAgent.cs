using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailHLAgent : StockAgentBase
    {
        public TrailHLAgent(StockContext context)
            : base(context)
        {
            Period = 13;
        }

        [StockAgentParam(2, 80)]
        public int Period { get; set; }

        public override string Description => "Buy with TRAILHL Stop";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        public override void Initialize(StockSerie stockSerie)
        {
            trailStop = stockSerie.GetTrailStop($"TRAILHL({Period})");
            bullEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenUp")];
            bearEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenDown")];
        }

        protected override TradeAction TryToOpenPosition()
        {
            int i = context.CurrentIndex;

            if (bullEvents[i])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition()
        {
            int i = context.CurrentIndex;

            if (bearEvents[i]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
