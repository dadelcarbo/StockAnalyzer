using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockPortfolio;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs.TradeManager
{
    public class TradeManagerViewModel : NotifyPropertyChangedBase
    {
        public TradeManagerViewModel(StockPortfolio portfolio, StockSerie serie)
        {
            this.Portfolio = new PortfolioViewModel(portfolio);
            this.StockSerie = serie;
        }

        public StockSerie StockSerie { get; set; }
        public PortfolioViewModel Portfolio { get; set; }

        #region REFRESH PORTFOLIO
        private CommandBase portfolioRefreshCmd;
        public ICommand PortfolioRefreshCmd => portfolioRefreshCmd ??= new CommandBase(PerformPortfolioRefreshCmd);

        private void PerformPortfolioRefreshCmd()
        {
            if (!this.Portfolio.RefreshPortfolio())
            {
                this.Portfolio = null;
            }
            this.OnPropertyChanged(nameof(Portfolio));
        }
        #endregion
    }
    public class PortfolioViewModel : NotifyPropertyChangedBase
    {
        StockPortfolio portfolio;

        public PortfolioViewModel(StockPortfolio portfolio)
        {
            this.portfolio = portfolio;
        }

        public bool RefreshPortfolio()
        {
            if (!portfolio.SaxoLogin())
                return false;

            this.portfolio.Refresh();

            return true;
        }

        [Property(null, "1-General")]
        public string Name => this.portfolio.Name;
        [Property(null, "1-General")]
        public float Balance => this.portfolio.Balance;
        [Property(null, "1-General")]
        public float AccountValue => this.portfolio.TotalValue;
        [Property(null, "1-General")]
        public float RiskFreeValue => this.portfolio.RiskFreeValue;
        [Property("P2", "1-General")]
        public float DayVar { get; private set; }
        [Property(null, "1-General")]
        public float DayPL { get; private set; }

    }
}
