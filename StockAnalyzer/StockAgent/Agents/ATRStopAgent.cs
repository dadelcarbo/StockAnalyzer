using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class ATRStopAgent : StockAgentBase
    {
        public ATRStopAgent(StockContext context)
            : base(context)
        {
            Period = 13;
            BBWidth = 2.0f;
        }

        [StockAgentParam(5, 80)]
        public int Period { get; set; }

        [StockAgentParam(0.5f, 4.0f)]
        public float BBWidth { get; set; }

        public override string Description => "Buy when Open and close are above EMA";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        public override void Initialize(StockSerie stockSerie)
        {
            trailStop = stockSerie.GetTrailStop($"TRAILATRBAND({Period},{BBWidth},{-BBWidth},MA)");
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
