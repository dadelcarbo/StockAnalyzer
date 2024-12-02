using Newtonsoft.Json;
using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockPortfolio;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StockAnalyzerApp.CustomControl.PortfolioDlg.TradeDlgs.TradeManager
{
    public class TradeManagerViewModel : NotifyPropertyChangedBase
    {
        #region EVENTS
        public delegate void OrdersChangedHandler(TradeManagerViewModel tradeManagerViewModel);
        public event OrdersChangedHandler OrdersChanged;
        public void RaiseOrdersChanged() { this.OrdersChanged?.Invoke(this); }
        #endregion

        public TradeManagerViewModel(StockPortfolio portfolio, StockSerie serie)
        {
            this.UseLog = true;
            this.Portfolio = new PortfolioViewModel(portfolio);
            this.StockSerie = serie;
            this.PortfolioRisk = portfolio.MaxRisk;
            this.isPortfolioRiskLocked = true;

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
                this.Bid = 0;
                this.Ask = 0;
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
        public float Bid
        {
            get => bid; set
            {
                if (SetProperty(ref bid, value))
                {
                    this.OnPropertyChanged(nameof(this.EntryAmount));
                    this.OnPropertyChanged(nameof(this.EntryPortfolioPercent));

                    if (bid > 0 && Qty == 0)
                    {
                        var nb = (int)(this.Portfolio.AccountValue / this.Portfolio.Portfolio.MaxPositions / this.bid) / 50;
                        this.Qty = nb * 50;
                    }
                }
            }
        }
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
        /// <summary>
        /// Qty= (Entry-Stop)/Risk
        /// </summary>
        public int Qty
        {
            get => qty;
            set
            {
                if (SetProperty(ref qty, value))
                {
                    if (this.qty > 0 && this.bid > 0)
                    {
                        if (this.IsPortfolioRiskLocked)
                        {
                            var minRisk = this.Portfolio.AccountValue * 0.0005f;
                            var maxRisk = this.Portfolio.AccountValue * this.Portfolio.MaxRisk;
                            this.EntryMinStop = this.Bid - maxRisk / this.qty;
                            this.EntryMaxStop = this.Bid - minRisk / this.qty;

                            this.EntryStop = this.Bid - risk / this.qty;
                            this.EntryStopPercent = (this.Bid - this.entryStop) / this.Bid;
                        }
                        else // Stop is locked
                        {
                            this.portfolioRisk = this.qty * (this.bid - this.EntryStop) / this.Portfolio.AccountValue;
                            this.OnPropertyChanged(nameof(this.PortfolioRisk));

                            var minRisk = this.Portfolio.AccountValue * 0.0005f;
                            var maxRisk = this.Portfolio.AccountValue * this.Portfolio.MaxRisk;
                            this.EntryMinStop = this.Bid - maxRisk / this.qty;
                            this.EntryMaxStop = this.Bid - minRisk / this.qty;
                        }
                    }
                    this.OnPropertyChanged(nameof(this.EntryAmount));
                    this.OnPropertyChanged(nameof(this.EntryPortfolioPercent));
                }
            }
        }

        private float portfolioRisk;
        /// <summary>
        /// Risk = Qty(Entry-Stop)
        /// </summary>
        [Property(null, "1-General")]
        public float PortfolioRisk
        {
            get => portfolioRisk;
            set
            {
                if (SetProperty(ref portfolioRisk, value))
                {
                    this.Risk = this.Portfolio.AccountValue * portfolioRisk;
                    if (this.qty > 0 && this.bid > 0)
                    {
                        var minRisk = this.Portfolio.AccountValue * 0.0005f;
                        var maxRisk = this.Portfolio.AccountValue * this.Portfolio.MaxRisk;
                        this.EntryMinStop = this.Bid - maxRisk / this.qty;
                        this.EntryMaxStop = this.Bid - minRisk / this.qty;

                        this.entryStop = this.Bid - risk / this.qty;
                        this.EntryStopPercent = (this.Bid - this.entryStop) / this.Bid;
                        this.OnPropertyChanged(nameof(this.EntryStop));
                        RaiseOrdersChanged();

                    }
                }
            }
        }

        private float risk;

        public float Risk { get => risk; set => SetProperty(ref risk, value); }

        private float entryStop;
        /// Stop = Entry - Risk/Qty
        /// </summary>
        public float EntryStop
        {
            get => entryStop;
            set
            {
                if (SetProperty(ref entryStop, value))
                {
                    if (this.qty > 0 && this.bid > 0)
                    {
                        RaiseOrdersChanged();
                    }
                    this.OnPropertyChanged(nameof(this.EntryAmount));
                    this.OnPropertyChanged(nameof(this.EntryPortfolioPercent));

                }
            }
        }

        private float entryStopPercent;
        public float EntryStopPercent { get => entryStopPercent; set => SetProperty(ref entryStopPercent, value); }

        private float entryMinStop;
        [Property(null, "1-General")]
        public float EntryMinStop { get => entryMinStop; set => SetProperty(ref entryMinStop, value); }

        private float entryMaxStop;
        [Property(null, "1-General")]
        public float EntryMaxStop { get => entryMaxStop; set => SetProperty(ref entryMaxStop, value); }

        [Property(null, "1-General")]
        public float EntryAmount => this.Bid * this.Qty;

        [Property("P2", "1-General")]
        public float EntryPortfolioPercent => this.EntryAmount / this.Portfolio.AccountValue;

        private bool isQtyLocked;
        [Property("P2", "1-General")]
        public bool IsQtyLocked { get => isQtyLocked; set => SetProperty(ref isQtyLocked, value); }

        private bool isPortfolioRiskLocked;
        [Property("P2", "1-General")]
        public bool IsPortfolioRiskLocked { get => isPortfolioRiskLocked; set => SetProperty(ref isPortfolioRiskLocked, value); }

        private bool isStopLocked;
        [Property("P2", "1-General")]
        public bool IsStopLocked { get => isStopLocked; set => SetProperty(ref isStopLocked, value); }
    }

    public class RadioButtonImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isChecked = (bool)value;
            var root = parameter.ToString();

            var imagePath = isChecked ? root + "LockClose.png" : root + "LockOpen.png";

            return new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
