using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class ATRStopAgent : StockAgentBase
    {
        public ATRStopAgent()
        {
            Period = 12;
            UpWidth = 2.0f;
        }

        [StockAgentParam(5, 80)]
        public int Period { get; set; }

        [StockAgentParam(0.5f, 4.0f)]
        public float UpWidth { get; set; }
        [StockAgentParam(0.5f, 4.0f)]
        public float DownWidth { get; set; }

        public override string Description => "Buy according to TrailATRBand";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override void Init(StockSerie stockSerie)
        {
            trailStop = stockSerie.GetTrailStop($"TRAILATRBAND({Period},{UpWidth},{-DownWidth},MA)");
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
