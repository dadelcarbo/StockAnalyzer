using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class BuyAgent : StockAgentBase
    {
        public BuyAgent()
        {
        }

        public override string Description => "Buy systemetically, never sells. It to test stop and target management";

        public override string DisplayIndicator => null;

        FloatSerie emaSerie;
        protected override bool Init()
        {
            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            return TradeAction.Buy;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            return TradeAction.Nothing;
        }
    }
}
