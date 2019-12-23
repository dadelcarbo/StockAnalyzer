using StockAnalyzer.StockPortfolio3;
using System.Collections.Generic;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    public class BinckPortfolioViewModel
    {
        public BinckPortfolioViewModel()
        {
        }

        public List<StockPortfolio> Portfolios => StockPortfolio.Portfolios;

        public StockPortfolio Portfolio
        {
            get { return StockAnalyzerForm.MainFrame.BinckPortfolio; }
            set
            {
                if (StockAnalyzerForm.MainFrame.BinckPortfolio != value)
                {
                    StockAnalyzerForm.MainFrame.BinckPortfolio = value;
                }
            }
        }
    }
}