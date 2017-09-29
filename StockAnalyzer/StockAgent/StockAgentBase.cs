using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent
{
    public abstract class StockAgentBase : IStockAgent
    {
        protected StockContext context;

        protected FloatSerie closeSerie;

        protected StockAgentBase(StockContext context)
        {
            this.context = context;
            this.closeSerie = context.Serie.GetSerie(StockDataType.CLOSE);
        }

        public TradeAction Decide()
        {
            TradeAction result = TradeAction.Nothing;
            switch (context.PositionStatus)
            {
                case StokPositionStatus.Closed:
                    result = this.TryToOpenPosition();
                    break;
                case StokPositionStatus.Opened:
                    result = this.TryToClosePosition();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("PositionType " + context.PositionStatus + " is not supported");
            }
            return result;
        }

        protected abstract TradeAction TryToClosePosition();

        protected abstract TradeAction TryToOpenPosition();
    }
}
