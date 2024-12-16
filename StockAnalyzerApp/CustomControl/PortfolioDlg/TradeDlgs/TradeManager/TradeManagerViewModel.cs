﻿using Newtonsoft.Json;
using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockPortfolio;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
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
            this.Bid = 0;
            this.Ask = 0;
            var priceInfo = await this.Portfolio.Portfolio.GetPriceAsync(this.StockSerie);
            if (priceInfo?.Quote != null)
            {
                this.Ask = priceInfo.Quote.Ask;
                this.Bid = priceInfo.Quote.Bid;
            }
            else
            {
                return;
            }

            return;


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
                MessageBox.Show(ex.Message);
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
                    if (bid > 0)
                    {
                        if (Qty == 0)
                        {
                            this.Qty = (int)(this.Portfolio.AccountValue / this.Portfolio.Portfolio.MaxPositions / this.bid);
                        }
                        this.EntryMaxStop = this.bid - this.PortfolioRiskEuro / this.MaxQty;
                        this.EntryMinStop = 0;
                        this.RaiseOrdersChanged();
                    }

                    this.OnPropertyChanged(nameof(EntryAmount));
                    this.OnPropertyChanged(nameof(EntryPortfolioPercent));
                    this.OnPropertyChanged(nameof(MaxQty));
                }
            }
        }
        #endregion

        #region Buy/Sell Commands
        private CommandBase sellCommand;
        public ICommand SellCommand => sellCommand ??= new CommandBase(Sell, CanSell, this, new[] { nameof(Ask) });

        private void Sell()
        {
        }

        private bool CanSell()
        {
            return ask > 0 && qty > 0;
        }

        private CommandBase buyCommand;
        public ICommand BuyCommand => buyCommand ??= new CommandBase(Buy, CanBuy, this, new[] { nameof(Bid) });

        private void Buy()
        {

        }

        private bool CanBuy()
        {
            return bid > 0 && qty > 0;
        }
        #endregion

        [Property(null, "1-General")]
        public int MaxQty => this.bid == 0 ? 0 : (int)Math.Floor(this.Portfolio.AccountValue * this.Portfolio.Portfolio.MaxPositionSize / this.bid);

        private int qty;
        /// <summary>
        /// Qty = (Entry-Stop)/Risk
        /// </summary>
        public int Qty
        {
            get => qty;
            set
            {
                var newQty = Math.Min(value, MaxQty);
                if (SetProperty(ref qty, newQty))
                {
                    if (this.qty == 0 || this.bid == 0)
                        return;

                    var minRisk = this.Portfolio.AccountValue * this.Portfolio.MinRisk;
                    var maxRisk = this.Portfolio.AccountValue * this.Portfolio.DynamicRisk;
                    var minQty = (int)Math.Floor(minRisk / (this.bid - this.entryStop));
                    this.qty = Math.Max(this.qty, minQty);

                    this.entryStop = Math.Max(this.entryMinStop, this.Bid - this.PortfolioRiskEuro / this.qty);

                    this.OnPropertyChanged(nameof(EntryStop));

                    this.OnPropertyChanged(nameof(EntryStopPercent));
                    this.OnPropertyChanged(nameof(EntryAmount));
                    this.OnPropertyChanged(nameof(EntryPortfolioPercent));
                    this.OnPropertyChanged(nameof(EntryRisk));

                    RaiseOrdersChanged();
                }
            }
        }

        [Property("F2", "1-General")]
        public float PortfolioRiskEuro => this.Portfolio.AccountValue * this.Portfolio.DynamicRisk;

        public float EntryRisk => (this.qty == 0 || this.bid == 0) ? 0 : (this.bid - this.entryStop) * this.qty;

        private float entryStop;
        /// <summary>
        /// Stop = Entry - Risk/Qty
        /// </summary>
        public float EntryStop
        {
            get => entryStop;
            set
            {
                if (SetProperty(ref entryStop, value))
                {
                    if (this.qty == 0 || this.bid == 0)
                        return;

                    this.qty = (int)Math.Floor(this.PortfolioRiskEuro / (this.bid - this.entryStop));
                    // TODO Cap to max position size

                    this.OnPropertyChanged(nameof(Qty));

                    this.OnPropertyChanged(nameof(EntryStopPercent));
                    this.OnPropertyChanged(nameof(EntryAmount));
                    this.OnPropertyChanged(nameof(EntryPortfolioPercent));
                    this.OnPropertyChanged(nameof(EntryRisk));

                    RaiseOrdersChanged();
                }
            }
        }

        public float EntryStopPercent => this.bid == 0 ? 0f : (this.bid - this.entryStop) / this.bid;

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
