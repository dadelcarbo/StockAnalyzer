using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class RSIAgent : StockAgentBase
    {
        public RSIAgent()
        {
            Period = 30;
            BuyLevel = 50;
            SellLevel = 50;
        }

        [StockAgentParam(10, 50, 1)]
        public int Period { get; set; }

        [StockAgentParam(10.0f, 90.0f, 0.05f)]
        public float BuyLevel { get; set; }

        [StockAgentParam(10.0f, 90.0f, 0.05f)]
        public float SellLevel { get; set; }

        public override string Description => "Buy when according to RSI signals";

        public override string DisplayIndicator => $"INDICATOR|RSI({Period},1)";

        FloatSerie rsiSerie;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            rsiSerie = stockSerie.GetIndicator($"RSI({Period},1)").Series[0];
            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (rsiSerie[index - 1] < this.BuyLevel && rsiSerie[index] >= this.BuyLevel)
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (rsiSerie[index] <= this.SellLevel)
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
