using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class StupidAgent : StockAgentBase
    {
        public StupidAgent(StockContext context)
            : base(context)
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
        protected override TradeAction TryToOpenPosition()
        {
            int i = context.CurrentIndex;
            float close = closeSerie[i];
            float high = closeSerie.GetMax(i - this.LookBack, i);
            float loss = (high - close) / high;
            if (loss > this.EntryPercentDown)
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }
 
        protected override TradeAction TryToClosePosition()
        {
            int i = context.CurrentIndex;
            float close = closeSerie[i];
            float gain = (close - context.Trade.EntryValue) / context.Trade.EntryValue;
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
