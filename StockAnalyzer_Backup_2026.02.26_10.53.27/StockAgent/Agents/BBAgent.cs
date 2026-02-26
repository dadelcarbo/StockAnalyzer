using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class BBAgent : StockAgentBase
    {
        public BBAgent()
        {
            Period = 13;
            UpWidth = 2.0f;
            DownWidth = 2.0f;
        }

        [StockAgentParam(5, 60, 5)]
        public int Period { get; set; }

        [StockAgentParam(0.5f, 4.0f, 0.5f)]
        public float UpWidth { get; set; }

        [StockAgentParam(0.5f, 4.0f, 0.5f)]
        public float DownWidth { get; set; }

        public override string Description => "Buy with BBStop";
        public override string DisplayIndicator => $"INDICATOR|BB({Period},{UpWidth},{-DownWidth},MA)";

        FloatSerie upBand;
        FloatSerie lowBand;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            upBand = stockSerie.GetIndicator($"BB({Period},{UpWidth},{-DownWidth},MA)").Series[0];
            lowBand = stockSerie.GetIndicator($"BB({Period},{UpWidth},{-DownWidth},MA)").Series[1];
            return upBand != null && lowBand != null;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (closeSerie[index] > upBand[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (closeSerie[index] < lowBand[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
