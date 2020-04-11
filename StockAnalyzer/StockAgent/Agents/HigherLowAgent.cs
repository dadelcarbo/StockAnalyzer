using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class HigherLowAgent : StockAgentBase
    {
        public HigherLowAgent(StockContext context)
            : base(context)
        {
            this.LookBack = 20;
        }

        public int LookBack { get; set; }

        [StockAgentParam(0.05f, 0.30f)]
        public float MaximumRisk { get; set; }
        [StockAgentParam(0.5f, 10f)]
        public float RiskRewardRatio { get; set; }
        [StockAgentParam(10f, 100f)]
        public int Period { get; set; }

        public override string Description => "Unknown";

        public float stopLoss;
        public float target;

        IStockIndicator indicator;
        public override void Initialize(StockSerie stockSerie)
        {
            base.Initialize(stockSerie);

            indicator = stockSerie.GetIndicator("OVERBOUGHTSR(STOKS(" + Period + "_3_3),75,25)");
        }
        protected override TradeAction TryToOpenPosition()
        {

            int i = context.CurrentIndex;
            if (indicator.Events[4][i]) // HigherLow occured
            {
                var supportSerie = indicator.Series[0];
                stopLoss = supportSerie[i - 1];
                int index = i - 2;
                while (float.IsNaN(stopLoss) && index > 0)
                {
                    stopLoss = supportSerie[index];
                    index--;
                }
                if (index > 0)
                {
                    float close = closeSerie[i];
                    if ((close - stopLoss) / close < this.MaximumRisk)
                    {
                        target = close + (close - stopLoss) * this.RiskRewardRatio;
                        return TradeAction.Buy;
                    }
                }
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition()
        {
            float close = closeSerie[context.CurrentIndex];

            if (close < stopLoss || close > target)
            {
                return TradeAction.Sell;
            }

            return TradeAction.Nothing;
        }
    }
}
