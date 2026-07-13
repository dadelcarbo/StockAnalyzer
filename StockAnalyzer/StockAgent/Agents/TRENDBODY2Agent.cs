using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TRENDBODY2Agent : StockAgentBase
    {
        public TRENDBODY2Agent()
        {
            UpPeriod = 50;
            DownPeriod = 25;
        }

        [StockAgentParam(5, 80, 5)]
        public int UpPeriod { get; set; }

        [StockAgentParam(5, 80, 5)]
        public int DownPeriod { get; set; }

        public override string Description => "Buy with TRENDBODY2 Bull Events, TREND body with different High/Low period";


        public override string DisplayIndicator => $"CLOUD|TRENDBODY2({UpPeriod},{DownPeriod})";


        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init()
        {
            if (DataSerie.Count < Math.Max(UpPeriod, DownPeriod))
                return false;

            var cloud = DataSerie.GetCloud($"TRENDBODY2({UpPeriod},{DownPeriod})");
            bullEvents = cloud.Events[Array.IndexOf(cloud.EventNames, "CloudUp")];
            bearEvents = cloud.Events[Array.IndexOf(cloud.EventNames, "CloudDown")];
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
