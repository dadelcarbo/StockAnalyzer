﻿using StockAnalyzer.StockClasses;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockAgent.Agents
{
    public class STOKFBODYAgent : StockAgentBase
    {
        public STOKFBODYAgent()
        {
            Period = 20;
            BullTrigger = 50f;
            BearTrigger = 50f;
        }

        [StockAgentParam(10, 120)]
        public int Period { get; set; }

        [StockAgentParam(40f, 100f)]
        public float BullTrigger { get; set; }

        [StockAgentParam(40f, 100f)]
        public float BearTrigger { get; set; }

        public override string Description => "Buy when according to STOKFBODY signals";

        FloatSerie stockfBodySerie;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            stockfBodySerie = stockSerie.GetIndicator($"STOKFBODY({Period})").Series[0];
            return true;
        }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if (stockfBodySerie[index - 1] < BullTrigger && stockfBodySerie[index] >= BullTrigger) // 1st bar above bull trigger
            {
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (stockfBodySerie[index - 1] > BearTrigger && stockfBodySerie[index] < BearTrigger) // 1st bar below bear trigger
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}