using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class SARAgent : StockAgentBase
    {
        [StockAgentParam(0.0f, 1f)]
        public float Speed { get; set; }

        public override string Description => "Buy with BBStop";
        public override string DisplayIndicator => $"INDICATOR|SAR(0.0,{Speed},0.2,1)";

        FloatSerie stopSerie;
        BoolSerie supportDetected;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < 50)
                return false;
            var sar = stockSerie.GetIndicator($"SAR(0.0,{Speed},0.2,1)");
            stopSerie = sar.Series[0];
            supportDetected = sar.Events[0];
            return stopSerie != null && supportDetected != null;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (supportDetected[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (closeSerie[index] < stopSerie[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
