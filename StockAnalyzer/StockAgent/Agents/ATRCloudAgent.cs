using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class ATRCloudAgent : StockAgentBase
    {
        public ATRCloudAgent()
        {
            Period = 12;
            Width = 2.0f;
        }

        [StockAgentParam(5, 80)]
        public int Period { get; set; }

        [StockAgentParam(0.5f, 4.0f)]
        public float Width { get; set; }

        public override string Description => "Buy when closing above ATR band and sell when closing below EMA";
        public override string DisplayIndicator => $"CLOUD|ATR({Period},{Width},{-Width},EMA)";


        FloatSerie buyTrigger;
        FloatSerie sellTrigger;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            var atrSerie = stockSerie.GetIndicator($"ATR({20})").Series[0];
            sellTrigger = stockSerie.GetIndicator($"EMA({Period})").Series[0];
            buyTrigger = sellTrigger + Width * atrSerie;
            return buyTrigger != null;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (closeSerie[index] > buyTrigger[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (closeSerie[index] < sellTrigger[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
