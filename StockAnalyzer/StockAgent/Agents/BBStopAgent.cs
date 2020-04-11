using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class BBStopAgent : StockAgentBase
    {
        public BBStopAgent(StockContext context)
            : base(context)
        {
            MAPeriod = 13;
            BBWidth = 2.0f;
        }

        [StockAgentParam(5, 60)]
        public int MAPeriod { get; set; }

        [StockAgentParam(0.75f, 3.0f)]
        public float BBWidth { get; set; }

        public override string Description => "Buy when Open and close are above EMA";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        public override void Initialize(StockSerie stockSerie)
        {
            trailStop = stockSerie.GetTrailStop($"TRAILBB({MAPeriod},{BBWidth},{-BBWidth})");
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
