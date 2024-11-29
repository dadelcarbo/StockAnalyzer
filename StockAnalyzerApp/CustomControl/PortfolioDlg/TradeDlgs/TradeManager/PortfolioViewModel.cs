using StockAnalyzer;
using StockAnalyzer.StockPortfolio;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs.TradeManager
{
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

        [Property("P2", "1-General")]
        public float MaxRisk => this.portfolio.MaxRisk;
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

        public StockPortfolio Portfolio =>  this.portfolio;
    }
}
