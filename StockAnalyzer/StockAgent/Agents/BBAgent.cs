using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class BBAgent : StockAgentBase
    {
        public BBAgent()
        {
            Period = 13;
            BBWidth = 2.0f;
        }

        [StockAgentParam(5, 60)]
        public int Period { get; set; }

        [StockAgentParam(0.75f, 3.0f)]
        public float BBWidth { get; set; }

        public override string Description => "Buy with BBStop";
        public override string DisplayIndicator => $"INDICATOR|BB({Period},{BBWidth},{-BBWidth},MA)";

        FloatSerie upBand;
        FloatSerie ma;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            upBand = stockSerie.GetIndicator($"BB({Period},{BBWidth},{-BBWidth},MA)").Series[0];
            ma = stockSerie.GetIndicator($"BB({Period},{BBWidth},{-BBWidth},MA)").Series[2];
            return upBand != null && ma != null;
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
            if (closeSerie[index] < ma[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
