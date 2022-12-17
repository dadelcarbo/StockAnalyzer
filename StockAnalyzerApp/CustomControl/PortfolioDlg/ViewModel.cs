using StockAnalyzer;
using StockAnalyzer.StockClasses;
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
                if (portfolioViewModel!= null && portfolioViewModel.IsDirty)
                {
                    portfolioViewModel.Portfolio.Serialize();
                }

                StockAnalyzerForm.MainFrame.Portfolio = value;
                portfolioViewModel = null;
                OnPropertyChanged(nameof(Portfolio));
                OnPropertyChanged(nameof(PortfolioViewModel));
            }
        }
        private PortfolioViewModel portfolioViewModel;
        public PortfolioViewModel PortfolioViewModel => portfolioViewModel == null ? portfolioViewModel = new PortfolioViewModel(Portfolio) : portfolioViewModel;
        static public IList<BarDuration> BarDurations => StockBarDuration.BarDurations;

    }
}