﻿using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems.StockClouds;
using StockAnalyzer.StockClasses.StockViewableItems.StockTrailStops;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class ATRReentryAgent : StockAgentBase
    {
        public ATRReentryAgent()
        {
            Period = 12;
            Width = 2.0f;
        }

        [StockAgentParam(1, 30)]
        public int Period { get; set; }

        [StockAgentParam(0.5f, 4.0f)]
        public float Width { get; set; }


        public override string Description => "Buy according to TrailATR CLOUD on re entry signal";

        public override string DisplayIndicator => $"CLOUD|TRAILATR(50,{Width},{-Width},EMA,{Period}";

        IStockCloud cloud;
        BoolSerie bullEvents;
        BoolSerie bearEvents;
        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < Period)
                return false;
            cloud = stockSerie.GetCloud($"TRAILATR(50,{Width},{-Width},EMA,{Period})");
            bullEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "Long Reentry")];
            bearEvents = cloud.Events[Array.IndexOf<string>(cloud.EventNames, "CloudDown")];
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
            if (bearEvents[index]) // bar fast below slow EMA
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}