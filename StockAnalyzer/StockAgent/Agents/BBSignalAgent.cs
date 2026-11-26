using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class BBAgentSignal : StockAgentBase
    {
        public BBAgentSignal()
        {
            Period = 35;
            UpWidth = 0.75f;
            DownWidth = 0.25f;
        }

        [StockAgentParam(15, 275, 5)]
        public int Period { get; set; }

        [StockAgentParam(0.5f, 4.0f, 0.5f)]
        public float UpWidth { get; set; }

        [StockAgentParam(-0.5f, 4.0f, 0.5f)]
        public float DownWidth { get; set; }

        [StockAgentParam(1, 12, 1)]
        public int SignalPeriod { get; set; }

        public override string Description => "Buy with Trail based on BB Signal";
        public override string DisplayIndicator => $"TRAILSTOP|TRAILBBSIGNAL({Period},{UpWidth},{DownWidth},EMA,{SignalPeriod})";

        BoolSerie isBullishEvents;

        protected override bool Init()
        {
            if (DataSerie.Count < Period)
                return false;

            isBullishEvents = DataSerie.GetTrailStop($"TRAILBBSIGNAL({Period},{UpWidth},{DownWidth},EMA,{SignalPeriod})").GetEvents("Bullish");

            return isBullishEvents != null;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (isBullishEvents[index])
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (!isBullishEvents[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
