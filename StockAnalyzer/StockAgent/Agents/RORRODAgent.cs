using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class RORRODAgent : StockAgentBase
    {
        public RORRODAgent()
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

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (rorFilterSerie[index] >= rodFilterSerie[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (rorFilterSerie[index] < rodFilterSerie[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
