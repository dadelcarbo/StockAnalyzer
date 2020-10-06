using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class BBStopAgent : StockAgentBase
    {
        public BBStopAgent()
        {
            MAPeriod = 13;
            UpBBWidth = 2.0f;
        }

        [StockAgentParam(5, 60)]
        public int MAPeriod { get; set; }

        [StockAgentParam(0.75f, 3.0f)]
        public float UpBBWidth { get; set; }

        [StockAgentParam(0.75f, 3.0f)]
        public float DownBBWidth { get; set; }

        public override string Description => "Buy with BBStop";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override void Init(StockSerie stockSerie)
        {
            trailStop = stockSerie.GetTrailStop($"TRAILBB({MAPeriod},{UpBBWidth},{-DownBBWidth})");
            bullEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenUp")];
            bearEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenDown")];
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
