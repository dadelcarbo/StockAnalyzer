using StockAnalyzer;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockPortfolio;
using System;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs.PortfolioRiskManager
{
    public class PositionViewModel : NotifyPropertyChangedBase
    {
        private StockPortofolio portfolio;

        public PositionViewModel(StockPortofolio portofolio, StockOrder entryOrder)
        {
            this.entryOrder = entryOrder;
            this.portfolio = portofolio;
        }

        #region Entry Order Wrapper
        private StockOrder entryOrder;
        public string StockName => entryOrder.StockName;
        public string Comment => entryOrder.Comment;
        public DateTime EntryDate => entryOrder.ExecutionDate;
        public float EntryValue => entryOrder.UnitCost;
        public float TotalValue => entryOrder.TotalCost;
        public int EntryQty => entryOrder.Number;
        #endregion
        
        public float CurrentValue { get; set; }

        public float PortfolioPercent { get { if (portfolio.TotalPortofolioValue==0) return 0; return entryOrder.TotalCost / portfolio.TotalPortofolioValue; } }

        private float? stop;
        public float? Stop { get { return stop; } set { if (stop == value) return; stop = value; OnPropertyChanged("Stop"); OnPropertyChanged("Risk"); OnPropertyChanged("PortfolioRisk"); } }

        public float? Risk { get { if (stop == null) return null; return (EntryValue - Stop) / EntryValue; } }
        public float? PortfolioRisk { get { if (stop == null) return null; return Risk*PortfolioPercent; } }
    }
}