using StockAnalyzer;
using StockAnalyzer.StockBinckPortfolio;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    public class BinckPortfolioViewModel : NotifyPropertyChangedBase
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

                    OnPropertyChanged(nameof(OpenedPositions));
                }
            }
        }

        public IEnumerable<StockPositionViewModel> OpenedPositions => Portfolio.Positions.Where(p => !p.IsClosed).OrderBy(p => p.StockName).Select(p=> new StockPositionViewModel(p));
    }
}