using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;

namespace StockAnalyzer.StockAgent
{
    public class StupidAgent : StockAgentBase
    {
        public StupidAgent(StockContext context)
            : base(context)
        {
        }

        protected override TradeAction TryToOpenPosition()
        {
            int i = context.CurrentIndex;
            float close = closeSerie[i];
            float high = closeSerie.GetMax(i - 20, i);
            float loss = (high - close) / high;
            if (loss > 0.2f)
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }
 
        protected override TradeAction TryToClosePosition()
        {
            int i = context.CurrentIndex;
            float close = closeSerie[i];
            float gain = (close - context.OpenValue) / context.OpenValue;
            if (gain > 0.2f)
            {
                return TradeAction.Sell;
            } 
            if (gain < -0.15f)
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        } 
    }
}
