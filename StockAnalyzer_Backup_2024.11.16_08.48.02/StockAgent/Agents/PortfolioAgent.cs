using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockViewableItems;
using StockAnalyzer.StockClasses.StockViewableItems.StockIndicators;
using StockAnalyzer.StockMath;
using System;

namespace StockAnalyzer.StockAgent.Agents
{
    public class PortfolioAgent : StockPortfolioAgentBase
    {
        public override string Description => "Agent to be used in Portfolio simulation only";

        public string EntryType { get; set; }
        public string EntryIndicator { get; set; }
        public string EntryEvent { get; set; }

        BoolSerie entryEvents { get; set; }

        public string FilterType { get; set; }
        public string FilterIndicator { get; set; }
        public string FilterEvent { get; set; }

        BoolSerie filterEvents { get; set; }

        public string ExitType { get; set; }
        public string ExitIndicator { get; set; }
        public string ExitEvent { get; set; }

        BoolSerie exitEvents { get; set; }

        public PositionManagement PositionManagement { get; set; }

        protected override bool Init(StockSerie stockSerie)
        {
            if (stockSerie.Count < 200)
                return false;
            if (EntryEvent == null)
                return false;
            var viewableSeries = stockSerie.GetViewableItem(this.EntryType.ToUpper() + "|" + this.EntryIndicator) as IStockEvent;
            if (viewableSeries != null)
            {
                this.entryEvents = viewableSeries.Events[Array.IndexOf<string>(viewableSeries.EventNames, this.EntryEvent)];
            }
            if (ExitEvent == null)
                return false;
            viewableSeries = stockSerie.GetViewableItem(this.ExitType.ToUpper() + "|" + this.ExitIndicator) as IStockEvent;
            if (viewableSeries != null)
            {
                this.exitEvents = viewableSeries.Events[Array.IndexOf<string>(viewableSeries.EventNames, this.ExitEvent)];
            }

            if (FilterEvent != null)
            {
                viewableSeries = stockSerie.GetViewableItem(this.FilterType.ToUpper() + "|" + this.FilterIndicator) as IStockEvent;
                if (viewableSeries != null)
                {
                    this.filterEvents = viewableSeries.Events[Array.IndexOf<string>(viewableSeries.EventNames, this.FilterEvent)];
                }
            }
            if (PositionManagement?.Rank == null) return false;
            var indicator = stockSerie.GetViewableItem("INDICATOR|" + this.PositionManagement.Rank) as IStockIndicator;
            if (indicator == null) return false;
            RankSerie = indicator.Series[0];

            return true;
        }

        float StopATR { get; set; }
        float HardStop { get; set; }

        protected override TradeAction TryToOpenPosition(int index)
        {
            if ((filterEvents == null || filterEvents[index]) && entryEvents[index])
            {
                if (StopATR != 0)
                {
                    this.HardStop = openSerie[index + 1] - StopATR * atrSerie[index];
                }
                return TradeAction.Buy;
            }
            return TradeAction.Nothing;
        }

        protected override TradeAction TryToClosePosition(int index)
        {
            if (this.HardStop != 0)
            {
                if (closeSerie[index] < HardStop)
                {
                    this.HardStop = 0;
                    return TradeAction.Sell;
                }
            }
            if (exitEvents[index])
            {
                return TradeAction.Sell;
            }
            return TradeAction.Nothing;
        }
    }
}
