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
            this.UseLog = true;
            this.Portfolio = new PortfolioViewModel(portfolio);
            this.StockSerie = serie;
            this.PortfolioRisk = portfolio.MaxRisk / 2.5f;

            Task.Run(() => this.PerformPriceRefreshCmd());
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
        #endregion

        #region Buy/Sell Commands
        private CommandBase sellCommand;
        public ICommand SellCommand => sellCommand ??= new CommandBase(Sell, CanSell, this);

        private void Sell()
        {
        }

        private bool CanSell()
        {
            return ask > 0 && qty > 0;
        }

        private CommandBase buyCommand;
        public ICommand BuyCommand => buyCommand ??= new CommandBase(Buy, CanBuy, this);

        private void Buy()
        {

        }

        private bool CanBuy()
        {
            return bid > 0 && qty > 0;
        }
        #endregion

        private int qty;
        public int Qty { get => qty; set => SetProperty(ref qty, value); }

        private float portfolioRisk;
        public float PortfolioRisk
        {
            get => portfolioRisk;
            set
            {
                SetProperty(ref portfolioRisk, value);
                this.Risk = this.Portfolio.AccountValue * portfolioRisk;
            }
        }

        private float risk;
        public float Risk { get => risk; set => SetProperty(ref risk, value); }

    }
}
