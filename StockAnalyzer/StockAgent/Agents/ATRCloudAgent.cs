using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
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
        public override string DisplayIndicator => $"CLOUD|ATR({Period},{Width},0,EMA)";


        IStockCloud cloud;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;

            var cloud = stockSerie.GetCloud($"ATR({Period},{Width},0,EMA)");
            bullEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "CloudUp")];
            bearEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "CloudDown")];
            return bullEvents != null && bearEvents != null;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (bullEvents[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (bearEvents[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
