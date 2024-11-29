using Newtonsoft.Json;
using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockPortfolio;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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

        #region REFRESH BID/ASK
        private ICommand priceRefreshCmd;
        public ICommand PriceRefreshCmd => priceRefreshCmd ??= new AsyncCommandBase(PerformPriceRefreshCmd);

        HttpClient httpClient = new HttpClient();
        private async Task PerformPriceRefreshCmd()
        {
            if (StockSerie.DataProvider != StockAnalyzer.StockClasses.StockDataProviders.StockDataProvider.SaxoIntraday)
                return;

            var url = $"https://fr-be.structured-products.saxo/page-api/instruments/v2/BE/details/isin/{this.StockSerie.ISIN}?locale=fr_BE";

            try
            {
                var resp = await httpClient.GetAsync(url);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    var saxoPrice = JsonConvert.DeserializeObject<SaxoPrice>(json);

                    var instrumentPrice = saxoPrice?.sections?.FirstOrDefault(s => s.section == "instrumentPrice");
                    if (instrumentPrice != null)
                    {
                        var askField = instrumentPrice.fields.FirstOrDefault(f => f.field == "ask");
                        if (askField != null)
                            this.Ask = askField.value.value;
                        var bidField = instrumentPrice.fields.FirstOrDefault(f => f.field == "bid");
                        if (bidField != null)
                            this.Bid = bidField.value.value;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private float ask;
        public float Ask { get => ask; set => SetProperty(ref ask, value); }

        private float bid;
        public float Bid { get => bid; set => SetProperty(ref bid, value); }

        private CommandBase sellCommand;
        public ICommand SellCommand => sellCommand ??= new CommandBase(Sell);

        private void Sell()
        {
        }

        private CommandBase buyCommand;
        public ICommand BuyCommand => buyCommand ??= new CommandBase(Buy);

        private void Buy()
        {
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
