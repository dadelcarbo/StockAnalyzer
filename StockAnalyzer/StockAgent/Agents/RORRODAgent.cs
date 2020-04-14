using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class RORRODAgent : StockAgentBase
    {
        public RORRODAgent(StockContext context)
            : base(context)
        {
            RORPeriod = 50;
            RORPeriod = 50;
        }

        [StockAgentParam(5, 200)]
        public int RORPeriod { get; set; }

        [StockAgentParam(5, 200)]
        public int RODPeriod { get; set; }

        public override string Description => "Buy when ROR > ROD";

        FloatSerie rorFilterSerie;
        FloatSerie rodFilterSerie;
        protected override void Init(StockSerie stockSerie)
        {
            rorFilterSerie = stockSerie.GetIndicator($"ROR({RORPeriod},1)").Series[0];
            rodFilterSerie = stockSerie.GetIndicator($"ROD({RORPeriod},1)").Series[0];
        }

        protected override TradeAction TryToOpenPosition()
        {
            int i = context.CurrentIndex;

            if (rorFilterSerie[i] >= rodFilterSerie[i])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition()
        {
            int i = context.CurrentIndex;

            if (rorFilterSerie[i] < rodFilterSerie[i])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
