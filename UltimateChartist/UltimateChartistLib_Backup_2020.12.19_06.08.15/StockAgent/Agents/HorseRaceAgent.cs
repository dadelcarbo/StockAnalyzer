using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;
using System.Linq;

namespace StockAnalyzer.StockAgent.Agents
{
    public class HorseRaceAgent : StockAgentBase
    {
        public HorseRaceAgent()
        {
            RankTrigger = 0.5f;
        }

        [StockAgentParam(0f, 1f)]
        public float RankTrigger { get; set; }

        public override string Description => "Buy with as an horse race by betting on the most performing once";

        FloatSerie rankSerie = null;
        protected override void Init(StockSerie stockSerie)
        {
            foreach (var serie in StockDictionary.Instance.Values.Where(s => s.BelongsToGroup(stockSerie.StockGroup)))
            {
                if (serie.Initialise())
                {
                    serie.BarDuration = stockSerie.BarDuration;
                }
            }
            rankSerie = stockSerie.GetIndicator("RANK(ROR(12_1))").Series[0];
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (rankSerie[index] >= RankTrigger)
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (rankSerie[index] < RankTrigger)
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
