using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class TFMidAgent : StockAgentBase
    {
        public TFMidAgent()
        {
            Period = 13;
            Trigger = 13;
        }

        [StockAgentParam(20, 80)]
        public int Trigger { get; set; }

        [StockAgentParam(2, 20)]
        public int Period { get; set; }

        public override string Description => "Buy with highest bar in 'Trigger' periods, and hold until TRENDBODY mid line not broken";

        public override string DisplayIndicator => $"CLOUD|TRENDBODY({Period})";

        FloatSerie midLine;
        FloatSerie highest;
        FloatSerie close;

        protected override void Init(StockSerie stockSerie)
        {
            midLine = stockSerie.GetCloud($"TRENDBODY({Period})").Series[2];
            highest = stockSerie.GetIndicator($"HIGHEST({Trigger})").Series[0];
            close = stockSerie.GetSerie(StockDataType.CLOSE);
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (highest[index]>Trigger)
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (close[index] < midLine[index]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
