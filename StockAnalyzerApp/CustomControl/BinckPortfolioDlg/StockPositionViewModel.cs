using StockAnalyzer.StockClasses;
using StockAnalyzer.StockBinckPortfolio;
using System;
using StockAnalyzer;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    public class StockPositionViewModel : NotifyPropertyChangedBase
    {
        BinckPortfolioViewModel portfolio;
        StockPosition position;
        public StockPositionViewModel(StockPosition p, BinckPortfolioViewModel portfolio)
        {
            this.position = p;
            this.portfolio = portfolio;
        }
        public bool IsValidName
        {
            get
            {
                var mapping = StockPortfolio.GetMapping(StockName);
                if (mapping == null)
                {
                    return StockDictionary.Instance.ContainsKey(position.StockName);
                }
                return StockDictionary.Instance.ContainsKey(mapping.StockName);
            }
        }

        public long Id => position.Id;
        public string StockName => position.StockName;

        #region TRADE ENTRY
        public DateTime EntryDate => position.EntryDate;
        public int EntryQty => position.EntryQty;
        public float EntryValue => Math.Abs(position.EntryValue);
        public float EntryCost => position.EntryCost;
        public string EntryComment
        {
            get => position.EntryComment;
            set { position.EntryComment = value; }
        }
        #endregion


        public float Stop { get { return position.Stop; } set { if (position.Stop != value) { position.Stop = value; OnPropertyChanged("Stop"); OnPropertyChanged("TradeRisk"); OnPropertyChanged("PortfolioRisk"); } } }

        public float TradeRisk => position.Stop == 0 ? 1.0f : (position.EntryValue - position.Stop) / position.EntryValue;
        public float PortfolioRisk => PortfolioPercent * (position.EntryValue - position.Stop) / position.EntryValue;

        public StockBarDuration BarDuration => position.BarDuration;
        public string Indicator => position.Indicator;

        public DateTime? ExitDate => position.ExitDate;
        public float? ExitValue => position.ExitValue;

        public float LastValue { get; set; }
        public float Variation => (LastValue - EntryValue) / (EntryValue);
        public float PortfolioVariation => PortfolioPercent * Variation;
        // @@@@ Need to use the most accurate portfolio value (Position value or Risk free value ?
        public float PortfolioPercent => this.portfolio.Portfolio.TotalValue > 0 ? ((EntryValue * this.EntryQty) / this.portfolio.Portfolio.InitialBalance) : 0.0f;
    }
}
