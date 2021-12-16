using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailEMACDAgent : StockAgentBase
    {
        public TrailEMACDAgent()
        {
            LongPeriod = 13;
        }

        [StockAgentParam(2, 120)]
        public int LongPeriod { get; set; }

        public int ShortPeriod => LongPeriod / 2;

        [StockAgentParam(2, 20)]
        public int SignalPriod { get; set; }

        public override string Description => "Buy with TrailEMACD Stop";
        public override string DisplayIndicator => $"TRAILSTOP|TRAILEMACD({LongPeriod},{ShortPeriod},{SignalPriod})";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < LongPeriod)
                return false;
            trailStop = stockSerie.GetTrailStop($"TRAILEMACD({LongPeriod},{ShortPeriod},{SignalPriod})");
            bullEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenUp")];
            bearEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenDown")];
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
