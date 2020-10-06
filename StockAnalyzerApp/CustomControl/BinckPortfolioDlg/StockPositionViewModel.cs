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
        public string StockName => position.StockName;
        public int Qty => position.Qty;
        public float Leverage => position.Leverage;
        public float OpenValue => Math.Abs(position.OpenValue);
        public DateTime StartDate => position.StartDate;
        public float LastValue { get; set; }
        public string Type => position.IsShort ? "Short" : "Long";
        public float Variation => position.IsShort && Leverage == 1 ? (OpenValue - LastValue) / (OpenValue) : (LastValue - OpenValue) / (OpenValue);
        public float PortfolioPercent => this.portfolio.Value > 0 ? ((LastValue * this.Qty) / this.portfolio.Value) : 0.0f;
    }
}
