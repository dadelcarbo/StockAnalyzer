using StockAnalyzer;
using StockAnalyzer.Portofolio;
using StockAnalyzer.StockPortfolio;
using System;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs.PortfolioRiskManager
{
    public class PositionViewModel : NotifyPropertyChangedBase
    {
        private StockPortofolio portfolio;

        private float riskTrigger = 0.01f;

        public PositionViewModel(StockPortofolio portofolio, StockOrder entryOrder)
        {
            this.entryOrder = entryOrder;
            this.portfolio = portofolio;
            this.PortfolioRisk = 0.01f;
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

        #region Portofolio Impact
        public float PortfolioPercent { get { if (portfolio.TotalPortofolioValue == 0) return 0; return entryOrder.TotalCost / portfolio.TotalPortofolioValue; } }

        public float Gain => (CurrentValue - EntryValue) / EntryValue;
        public float PortofolioGain => Gain * PortfolioPercent;

        public bool PortofolioExtraLoss => PortofolioGain < -riskTrigger;
        public bool PortofolioExtraGain => PortofolioGain > riskTrigger;

        #endregion

        #region Risk Management
        private float? stop;
        public float? Stop
        {
            get { return stop; }
            set
            {
                if (stop == value) return;
                stop = value;
                portfolioRisk = null;
                OnPropertyChanged("Stop"); OnPropertyChanged("Risk"); OnPropertyChanged("PortfolioRisk"); OnPropertyChanged("PortofolioExtraRisk");
            }
        }
        public float? Risk { get { if (stop == null) return null; return (EntryValue - Stop) / EntryValue; } }

        private float? portfolioRisk;
        public float? PortfolioRisk
        {
            get
            {
                if (portfolioRisk != null)
                {
                    return portfolioRisk;
                }
                if (stop == null) return null; return Risk * PortfolioPercent;
            }
            set
            {
                if (value == portfolioRisk) return;
                portfolioRisk = value;
                Stop = EntryValue * (1 - (portfolioRisk / PortfolioPercent));
            }
        }
        public bool PortofolioExtraRisk => PortfolioRisk > riskTrigger;
        #endregion
    }
}