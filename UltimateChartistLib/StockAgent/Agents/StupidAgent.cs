using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent.Agents
{
    public class StupidAgent : StockAgentBase
    {
        public StupidAgent()
        {
            this.LookBack = 20;
        }

        public override string Description => "Buys and sell on a percent trigger";

        public int LookBack { get; set; }

        [StockAgentParam(0.01f, 0.3f)]
        public float EntryPercentDown { get; set; }
        [StockAgentParam(0.01f, 0.3f)]
        public float ExitPercentDown { get; set; }
        [StockAgentParam(0.01f, 0.3f)]
        public float ExitPercentUp { get; set; }

        protected override void Init(StockSerie stockSerie)
        { }
        protected override TradeAction TryToOpenPosition(int index)
        {
            float close = closeSerie[index];
            float high = closeSerie.GetMax(index - this.LookBack, index);
            float loss = (high - close) / high;
            if (loss > this.EntryPercentDown)
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }
 
        protected override TradeAction TryToClosePosition(int index)
        {
            float close = closeSerie[index];
            float gain = (close - this.Trade.EntryValue) / this.Trade.EntryValue;
            if (gain > this.ExitPercentUp)
            {
                return TradeAction.Sell;
            } 
            if (gain < -this.ExitPercentDown)
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
