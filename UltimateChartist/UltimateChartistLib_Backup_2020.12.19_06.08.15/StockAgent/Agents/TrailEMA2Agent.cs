using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TrailEMA2Agent : StockAgentBase
    {
        public TrailEMA2Agent()
        {
            Period = 13;
        }

        [StockAgentParam(2, 120)]
        public int Period { get; set; }

        public override string Description => "Buy with TRAILEMA securing half position when reaching stop limit";

        IStockTrailStop trailStop;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        FloatSerie longStopSerie;
        FloatSerie shortStopSerie;

        float stop, target;
        protected override void Init(StockSerie stockSerie)
        {
            trailStop = stockSerie.GetTrailStop($"TRAILEMA({Period})");
            longStopSerie = trailStop.Series[0];
            shortStopSerie = trailStop.Series[1];
            bullEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenUp")];
            bearEvents = trailStop.Events[Array.IndexOf<string>(trailStop.EventNames, "BrokenDown")];
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (bullEvents[index])
            {
                stop = longStopSerie[index];
                target = closeSerie[index] + (closeSerie[index] - stop);
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (closeSerie[index] < stop)
                return TradeAction.Sell;

            if (bullEvents[index])
            {
                stop = longStopSerie[index];
            }
            if (!this.Trade.IsPartlyClosed && closeSerie[index] > target)
                return TradeAction.PartSell;

            //if (bearEvents[index]) // bar fast below slow EMA
            //{
            //    return TradeAction.Sell;
            //}
            return TradeAction.Nothing;
        }
    }
}
