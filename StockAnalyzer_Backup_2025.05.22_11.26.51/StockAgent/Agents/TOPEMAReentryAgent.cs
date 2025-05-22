using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TOPEMAReentryAgent : StockAgentBase
    {
        public TOPEMAReentryAgent()
        {
            Smoothing = 4;
        }

        [StockAgentParam(10, 40, 1)]
        public int Smoothing { get; set; }

        public override string Description => "Buy when TOPEMA signals reentry and sell when TOPEMA Support broken";

        public override string DisplayIndicator => $"TRAILSTOP|TOPEMA(175,35,{Smoothing})";

        FloatSerie supportSerie;
        FloatSerie resistanceSerie;
        IStockEvent events;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Smoothing)
                return false;

            events = stockSerie.GetIndicator(DisplayIndicator);
            supportSerie = stockSerie.GetIndicator(DisplayIndicator).Series[0];
            resistanceSerie = stockSerie.GetIndicator(DisplayIndicator).Series[1];

            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (events.Events[2][index] && !events.Events[4][index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (events.Events[3][index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
