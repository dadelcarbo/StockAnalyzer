using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System.Reflection;

namespace StockAnalyzer.StockAgent
{
    public abstract class StockAgentMoneyManagedBase : StockAgentBase
    {
        protected StockAgentMoneyManagedBase(StockContext context) : base(context)
        {
        }
        /// <summary>
        /// Stop value in percent 0.1 = 10%
        /// </summary>
        [StockAgentParam(0.03f, 0.15f)]
        public float Stop { get; set; }
        /// <summary>
        /// Target value in percent 0.1 = 10%
        /// </summary>
        [StockAgentParam(0.03f, 0.15f)]
        public float Target { get; set; }

        public override TradeAction Decide()
        {
            if (context.Trade == null)
            {
                return this.TryToOpenPosition();
            }
            else
            {
                var action = MoneyManagement();
                return action != TradeAction.Nothing ? action : this.TryToClosePosition();
            }
        }

        public TradeAction MoneyManagement()
        {
            int i = context.CurrentIndex;
            float close = closeSerie[i];
            float gain = (close - context.Trade.EntryValue) / context.Trade.EntryValue;
            if (gain > this.Target)
            {
                return TradeAction.Sell;
            }
            if (gain < -this.Stop)
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
