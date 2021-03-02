using StockAnalyzer;
using StockAnalyzer.StockBinckPortfolio;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.BinckPortfolioDlg
{
    public class ViewModel : NotifyPropertyChangedBase
    {
        public ViewModel()
        {
            this.Portfolios = StockPortfolio.Portfolios;
        }

        public IEnumerable<StockPortfolio> Portfolios { get; set; }

        public StockPortfolio Portfolio
        {
            get { return StockAnalyzerForm.MainFrame.BinckPortfolio; }
            set
            {
                if (StockAnalyzerForm.MainFrame.BinckPortfolio != value)
                {
                    StockAnalyzerForm.MainFrame.BinckPortfolio = value;
                    OnPropertyChanged(nameof(Portfolio));
                    OnPropertyChanged(nameof(PortfolioViewModel));
                }
            }
        }
        public BinckPortfolioViewModel PortfolioViewModel => new BinckPortfolioViewModel(Portfolio);
    }
}