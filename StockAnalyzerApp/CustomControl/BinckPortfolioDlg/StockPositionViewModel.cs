using StockAnalyzer.StockClasses;
using StockAnalyzer.StockBinckPortfolio;
using System;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    public class StockPositionViewModel
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

        public int Id => position.Id;
        public string StockName => position.StockName;

        #region TRADE ENTRY
        public DateTime EntryDate => position.EntryDate;
        public int EntryQty => position.EntryQty;
        public float EntryValue => Math.Abs(position.EntryValue);
        public float EntryCost => position.EntryCost;
        public float Leverage => position.Leverage;
        public string EntryComment
        {
            get => position.EntryComment;
            set { position.EntryComment = value; }
        }
        #endregion

        public float Stop => position.Stop;

        public StockBarDuration BarDuration => position.BarDuration;
        public string Indicator => position.Indicator;

        public DateTime? ExitDate => position.ExitDate;

        public float LastValue { get; set; }
        public string Type => position.IsShort ? "Short" : "Long";
        public float Variation => position.IsShort && Leverage == 1 ? (EntryValue - LastValue) / (EntryValue) : (LastValue - EntryValue) / (EntryValue);
        public float PortfolioPercent => this.portfolio.Value > 0 ? ((LastValue * this.EntryQty) / this.portfolio.Value) : 0.0f;
    }
}
