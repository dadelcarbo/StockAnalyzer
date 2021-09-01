using StockAnalyzer;
using StockAnalyzer.StockPortfolio;
using System.Collections.Generic;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg
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
            get { return StockAnalyzerForm.MainFrame.Portfolio; }
            set
            {
                if (StockAnalyzerForm.MainFrame.Portfolio != value)
                {
                    StockAnalyzerForm.MainFrame.Portfolio = value;
                    OnPropertyChanged(nameof(Portfolio));
                    OnPropertyChanged(nameof(PortfolioViewModel));
                }
            }
        }
        public PortfolioViewModel PortfolioViewModel => new PortfolioViewModel(Portfolio);
    }
}