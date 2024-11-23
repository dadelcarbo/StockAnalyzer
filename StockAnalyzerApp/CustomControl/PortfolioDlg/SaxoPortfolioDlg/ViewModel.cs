using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockPortfolio;
using StockAnalyzer.StockPortfolio.Saxo;
using System.Collections.Generic;
using System.Linq;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.SaxoPortfolioDlg
{
    public class ViewModel : NotifyPropertyChangedBase
    {
        public ViewModel()
        {
            this.Portfolios = null;
        }

        public IEnumerable<StockPortfolio> Portfolios { get; set; }

        public StockPortfolio Portfolio
        {
            get { return StockAnalyzerForm.MainFrame.Portfolio; }
            set
            {
                if (StockAnalyzerForm.MainFrame.Portfolio == value)
                    return;
                StockAnalyzerForm.MainFrame.Portfolio.Refreshed -= Portfolio_Refreshed;

                if (portfolioViewModel != null && portfolioViewModel.IsDirty)
                {
                    portfolioViewModel.Portfolio.Serialize();
                }
                StockAnalyzerForm.MainFrame.Portfolio = value;
                portfolioViewModel = null;
                OnPropertyChanged(nameof(Portfolio));
                OnPropertyChanged(nameof(PortfolioViewModel));

                value.Refreshed += Portfolio_Refreshed;
            }
        }

        public void Portfolio_Refreshed(StockPortfolio sender)
        {
            portfolioViewModel = null;
            OnPropertyChanged(nameof(Portfolio));
            OnPropertyChanged(nameof(PortfolioViewModel));
        }

        private PortfolioViewModel portfolioViewModel;
        public PortfolioViewModel PortfolioViewModel => portfolioViewModel == null ? portfolioViewModel = new PortfolioViewModel(Portfolio) : portfolioViewModel;
        static public IList<BarDuration> BarDurations => StockBarDuration.BarDurations;

        static public IEnumerable<string> Themes => StockAnalyzerForm.MainFrame.Themes.Append(string.Empty);
    }
}