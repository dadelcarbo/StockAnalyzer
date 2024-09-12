using Saxo.OpenAPI.AuthenticationServices;
using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace StockAnalyzer.StockPortfolio.Saxo
{
    public class SaxoPortfolio
    {
        const string PORTFOLIO_FILE_EXT = ".sptf";
        public string Name { get; set; }
        public string SaxoAccountId { get; set; }
        public string SaxoClientId { get; set; }
        public bool IsSaxoSimu { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastSyncDate { get; set; }
        public float InitialBalance { get; set; }
        public float Balance { get; set; }

        public float PositionValue { get; set; }
        [JsonIgnore]
        public float TotalValue => this.Balance + this.PositionValue;


        public float DrawDown => (MaxValue - TotalValue) / MaxValue;
        public float MaxValue { get; set; }

        public float MaxRisk { get; set; }

        public float AutoTradeRisk { get; set; } = 0.001f;
        public float MaxPositionSize { get; set; }

        [JsonIgnore]
        public List<SaxoOrder> SaxoOpenOrders { get; } = new List<SaxoOrder>();

        public List<SaxoOrder> SaxoOrders { get; } = new List<SaxoOrder>();



        #region SAXO Integration Management

        private static readonly SortedDictionary<long, StockSerie> UicToSerieCache = new SortedDictionary<long, StockSerie>();

        public StockSerie GetStockSerieFromUic(long uic)
        {
            using var ml = new MethodLogger(this);
            if (UicToSerieCache.ContainsKey(uic))
                return UicToSerieCache[uic];

            StockSerie stockSerie = null;
            var instrument = instrumentService.GetInstrumentById(uic);
            if (instrument == null)
            {
                stockSerie = new StockSerie(uic.ToString(), uic.ToString(), StockSerie.Groups.ALL, StockDataProvider.Generated, BarDuration.Daily);
                UicToSerieCache.Add(uic, stockSerie);
                StockLog.Write($"Instrument: {uic} not found !");
                return stockSerie;
            }

            // Find StockSerie by ISIN
            if (!string.IsNullOrEmpty(instrument.Isin))
            {
                stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == instrument.Isin);
                if (stockSerie != null)
                {
                    UicToSerieCache.Add(uic, stockSerie);
                    return stockSerie;
                }
                stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == instrument.Isin);
                if (stockSerie != null)
                {
                    UicToSerieCache.Add(uic, stockSerie);
                    return stockSerie;
                }
            }

            // Find instrument in stock Dictionnary by Symbol
            var symbol = instrument.Symbol.Split(':')[0];
            var stockName = instrument.Description.ToUpper().Replace("SA", "").Replace("SCA", "").Trim();
            stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => (s.Symbol == symbol) || s.StockName == stockName);
            if (stockSerie == null)
            {
                if (instrument.ExchangeId == "CATS_SAXO" || instrument.AssetType == "WarrantOpenEndKnockOut")
                {
                    stockSerie = new StockSerie(instrument.Description, symbol, StockSerie.Groups.TURBO, StockDataProvider.SaxoIntraday, BarDuration.H_1);
                    stockSerie.ISIN = symbol;
                }
            }
            if (string.IsNullOrEmpty(instrument.Isin) && !string.IsNullOrEmpty(stockSerie?.ISIN))
            {
                instrument.Isin = stockSerie.ISIN;
                instrumentService.GetInstrumentByIsin(instrument.Isin);
            }
            UicToSerieCache.Add(uic, stockSerie);
            return stockSerie;
        }

        Account account = null;
        readonly AccountService accountService = new AccountService();
        readonly InstrumentService instrumentService = new InstrumentService();
        readonly OrderService orderService = new OrderService();
        private string name;

        public bool SaxoLogin()
        {
            using var ml = new MethodLogger(this, true, this.Name);

            if (string.IsNullOrEmpty(SaxoAccountId))
            {
                MessageBox.Show("Missing Saxo Account Id, check portfolio file", "Saxo connection exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrEmpty(SaxoClientId))
            {
                MessageBox.Show("Missing Saxo Client Id, check portfolio file", "Saxo connection exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            var saxoSession = LoginService.Login(this.SaxoClientId, Folders.Portfolio, this.IsSaxoSimu);
            if (saxoSession == null)
                return false;

            if (account == null)
            {
                account = accountService.GetAccounts()?.FirstOrDefault(a => a.AccountId == this.SaxoAccountId);

                if (account == null)
                {
                    MessageBox.Show($"Account: {this.SaxoAccountId} not found !", "Porfolio sync error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }
        public bool SaxoSilentLogin()
        {
            using var ml = new MethodLogger(this, true, this.Name);
            if (string.IsNullOrEmpty(SaxoAccountId) || string.IsNullOrEmpty(SaxoClientId))
            {
                return false;
            }
            var saxoSession = LoginService.SilentLogin(this.SaxoClientId, Folders.Portfolio, this.IsSaxoSimu);
            if (saxoSession == null)
                return false;

            StockLog.Write($"Silent login success for portfolio: {this.Name}");
            if (account == null)
            {
                account = accountService.GetAccounts()?.FirstOrDefault(a => a.AccountId == this.SaxoAccountId);
                if (account == null)
                {
                    return false;
                }
            }
            return true;
        }
        public Performance Performance { get; set; }
        public void Refresh()
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoSilentLogin())
                    return;

                // Update portfolio balance
                var balance = accountService.GetBalance(account);
                if (balance != null)
                {
                    this.Balance = balance.CashAvailableForTrading;
                    this.PositionValue = balance.UnrealizedPositionsValue;
                    this.MaxValue = Math.Max(this.MaxValue, balance.TotalValue);
                }

                // Get Opened Orders
                this.SaxoOpenOrders.Clear();
                var saxoOpenedOrders = orderService.GetOpenedOrders(account);
                if (saxoOpenedOrders != null && saxoOpenedOrders.Length > 0)
                {
                    foreach (var order in saxoOpenedOrders)
                    {
                        var stockSerie = GetStockSerieFromUic(order.Uic);
                        var saxoOrder = new SaxoOrder(order);
                        saxoOrder.StockName = stockSerie == null ? order.Uic.ToString() : stockSerie.StockName;

                        this.SaxoOpenOrders.Add(saxoOrder);
                    }
                }

                // Get Opened Positions

                //// Check activity Orders
                //var upToDate = DateTime.Today;
                //var fromDate = this.LastSyncDate;
                //var toDate = DateTime.Today.AddDays(1);
                //var orders = orderService.GetOrderActivities(account, fromDate, toDate);
                //if (orders != null && orders.Count > 0)
                //{
                //    foreach (var op in orders.OrderBy(o => o.LogId).Where(o => o.LogId > this.LastLogId).ToList())
                //    {
                //        this.AddSaxoActivityOrder(op);
                //    }
                //}

                this.LastSyncDate = DateTime.Today;
                this.Serialize();

                //  this.Refreshed?.Invoke(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Porfolio sync error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void GetPerformance()
        {
            if (!this.SaxoLogin())
                return;

            var lastCacDate = StockDictionary.Instance["CAC40"].LastValue.DATE.Date;
            if (this.Performance?.Balance?.AccountValue == null || this.Performance.Balance.AccountValue.Last().Date < lastCacDate)
            {
                this.Performance = accountService.GetPerformance(account);
                this.MaxValue = Math.Max(this.MaxValue, this.Performance.Balance.AccountValue.Max(v => v.Value));
            }
        }


        public Position[] SaxoGetPositions()
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                return accountService?.GetPositions(account);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public OpenedOrder[] SaxoGetOrders(long uic = 0)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                return orderService?.GetOpenedOrders(account, uic);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public OrderActivity[] SaxoGetOrderActivities(long orderId)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                return orderService?.GetOrderActivities(account, orderId)?.Data;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Position SaxoGetPosition(StockSerie stockSerie)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                    return null;

                return accountService?.GetPositions(account).FirstOrDefault(p => p.PositionBase.Uic == instrument.Identifier);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public Position SaxoGetPosition(long positionId)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                return accountService?.GetPositionById(account, positionId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public long SaxoBuyOrder(StockSerie stockSerie, OrderType orderType, int qty, float stopValue = 0, float orderValue = 0, bool t1 = false)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return 0;

                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }
                decimal stop = instrumentDetail.RoundToTickSize(stopValue);

                OrderResponse orderResponse = null;
                switch (orderType)
                {
                    case OrderType.Market:
                        orderResponse = orderService.BuyMarketOrder(account, instrument, qty, stop);
                        break;
                    case OrderType.Limit:
                        decimal limit = instrumentDetail.RoundToTickSize(orderValue);
                        orderResponse = orderService.BuyLimitOrder(account, instrument, qty, limit, stop, t1);
                        break;
                    case OrderType.Threshold:
                        decimal threshold = instrumentDetail.RoundToTickSize(orderValue);
                        orderResponse = orderService.BuyTresholdOrder(account, instrument, qty, threshold, stop);
                        break;
                    default:
                        break;
                }

                this.Refresh();
                if (!string.IsNullOrEmpty(orderResponse?.OrderId))
                {
                    return long.Parse(orderResponse?.OrderId);
                }
            }
            catch (SaxoApiException ex)
            {
                MessageBox.Show(ex.ErrorInfo.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return 0;
        }
        public long SaxoSellOrder(StockSerie stockSerie, OrderType orderType, int qty, float orderValue = 0)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return 0;

                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return 0;
                }

                OrderResponse orderResponse = null;
                switch (orderType)
                {
                    case OrderType.Market:
                        orderResponse = orderService.SellMarketOrder(account, instrument, qty);
                        break;
                    case OrderType.Limit:
                        decimal limit = instrumentDetail.RoundToTickSize(orderValue);
                        orderResponse = orderService.SellLimitOrder(account, instrument, qty, limit);
                        break;
                    case OrderType.Threshold:
                        decimal threshold = instrumentDetail.RoundToTickSize(orderValue);
                        orderResponse = orderService.SellStopOrder(account, instrument, qty, threshold);
                        break;
                    default:
                        break;
                }
                this.Refresh();
                return long.Parse(orderResponse?.OrderId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return 0;
        }
        public string SaxoUpdateStopOrder(StockPosition position, float exitValue)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentById(position.Uic);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument:{position.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {position.StockName}:{instrument.Symbol} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                decimal value = instrumentDetail.RoundToTickSize(exitValue);
                OrderResponse orderResponse = null;
                if (position.TrailStopId == 0)
                {
                    orderResponse = orderService.SellStopOrder(account, instrument, position.EntryQty, value);
                    position.TrailStopId = orderResponse?.OrderId == null ? 0 : long.Parse(orderResponse.OrderId);
                }
                else
                {
                    orderResponse = orderService.PatchOrder(account, instrument, position.TrailStopId, SaxoOrderType.StopIfTraded.ToString(), "Sell", position.EntryQty, value);
                }
                if (!string.IsNullOrEmpty(orderResponse?.OrderId))
                {
                    position.TrailStop = (float)value;
                    if (position.Stop == 0) { position.Stop = (float)value; }
                    this.Serialize();
                }
                return orderResponse?.OrderId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        public long SaxoClosePosition(long positionId)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoSilentLogin())
                    return 0;

                var position = this.SaxoGetPosition(positionId);
                if (position == null)
                    return 0;

                // Close related Orders
                var openOrders = this.SaxoGetOrders(position.PositionBase.Uic);
                if (openOrders != null)
                {
                    foreach (var order in openOrders.Where(o => o.BuySell == "Sell"))
                    {
                        orderService.CancelOrder(account, order.OrderId);
                    }
                }

                var instrument = this.GetInstrument(position.PositionBase.Uic);
                OrderResponse orderResponse = orderResponse = orderService.SellMarketOrder(account, instrument, (int)position.PositionBase.Amount);
                return orderResponse?.OrderId == null ? 0 : long.Parse(orderResponse.OrderId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return 0;
        }

        public string SaxoClosePosition(StockPosition position, OrderType orderType, float exitValue = 0.0f)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentById(position.Uic);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument:{position.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {position.StockName}:{instrument.Symbol} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                OrderResponse orderResponse = null;
                switch (orderType)
                {
                    case OrderType.Market:
                        if (position.TrailStopId != 0)
                        {
                            if (!orderService.CancelOrder(account, position.TrailStopId))
                                return null;
                        }
                        if (position.LimitOrderId != 0)
                        {
                            if (!orderService.CancelOrder(account, position.LimitOrderId))
                                return null;
                        }
                        orderResponse = orderService.SellMarketOrder(account, instrument, position.EntryQty);
                        break;
                    case OrderType.Limit:
                        decimal value = instrumentDetail.RoundToTickSize(exitValue);
                        if (position.LimitOrderId != 0)
                        {
                            orderResponse = orderService.PatchOrder(account, instrument, position.LimitOrderId, SaxoOrderType.Market.ToString(), "Sell", position.EntryQty, 0);
                        }
                        else
                        {
                            orderResponse = orderService.SellLimitOrder(account, instrument, position.EntryQty, instrumentDetail.RoundToTickSize(exitValue));
                            long.TryParse(orderResponse?.OrderId, out var id);
                            position.LimitOrderId = id;
                        }
                        break;
                    case OrderType.Threshold:
                        break;
                }
                return orderResponse?.OrderId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        public string SaxoUpdateOpenOrder(StockOpenedOrder openOrder, float newValue)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentById(openOrder.Uic);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument:{openOrder.StockName} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {openOrder.StockName}:{instrument.Symbol} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                decimal value = instrumentDetail.RoundToTickSize(newValue);
                OrderResponse orderResponse = null;
                orderResponse = orderService.PatchOrder(account, instrument, openOrder.Id, openOrder.OrderType, openOrder.BuySell, openOrder.Qty, value);
                if (!string.IsNullOrEmpty(orderResponse?.OrderId))
                {
                    openOrder.Value = (float)value;
                    this.Serialize();
                }
                return orderResponse?.OrderId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
        public void SaxoCancelOpenOrder(long orderId)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (orderId <= 0)
                    return;
                if (!this.SaxoLogin())
                    return;

                if (orderService.CancelOrder(account, orderId))
                {
                    this.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cancel order exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return;
        }

        public Instrument GetInstrument(long uic)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                return instrumentService.GetInstrumentById(uic);
            }
            catch (Exception)
            {
            }
            return null;
        }
        public InstrumentDetails GetInstrumentDetails(StockSerie stockSerie)
        {
            using var ml = new MethodLogger(this, true, this.Name);
            try
            {
                if (!this.SaxoLogin())
                    return null;

                var instrument = instrumentService.GetInstrumentByIsin(stockSerie.ISIN == null ? stockSerie.Symbol : stockSerie.ISIN);
                if (instrument == null)
                {
                    MessageBox.Show($"Instrument: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                var instrumentDetail = instrumentService.GetInstrumentDetailsById(instrument.Identifier, instrument.AssetType, account);
                if (instrumentDetail == null)
                {
                    MessageBox.Show($"InstrumentDetails: {stockSerie.StockName}:{stockSerie.ISIN} not found !", "Buy order exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                return instrumentDetail;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }

        #endregion

        private static bool archivePortfolioFile = false;
        public void Serialize()
        {
            lock (this)
            {
                using var ml = new MethodLogger(this, true, this.Name);

                string filepath = Path.Combine(Folders.Portfolio, this.Name + PORTFOLIO_FILE_EXT);

                // Save result for analysis purpose
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    Converters = { new JsonStringEnumConverter(), jsonSerializerSettings },
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };

                File.WriteAllText(filepath, JsonSerializer.Serialize(this, options));

                if (archivePortfolioFile)
                {
                    // Archive PTF files
                    string archiveDirectory = Path.Combine(Folders.Portfolio, "Archive");
                    if (!Directory.Exists(archiveDirectory))
                        Directory.CreateDirectory(archiveDirectory);
                    else
                    {
                        var dateLimit = DateTime.Today.AddDays(-2);
                        foreach (var file in Directory.EnumerateFiles(archiveDirectory).Where(f => File.GetLastWriteTime(f) < dateLimit))
                        {
                            File.Delete(file);
                        }
                    }
                    var fileDate = File.GetLastWriteTime(filepath);
                    var archiveFilePath = Path.Combine(archiveDirectory, this.Name + "_" + fileDate.ToString("yyyy_MM_dd HH_mm_ss.ff") + PORTFOLIO_FILE_EXT);
                    File.Copy(filepath, archiveFilePath);
                }
            }
        }
        static readonly CustomDateTimeConverter jsonSerializerSettings = new CustomDateTimeConverter(@"yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffZ");
        public static SaxoPortfolio Deserialize(string filepath)
        {
            try
            {
                using var ml = new MethodLogger(typeof(SaxoPortfolio));

                // Save result for analysis purpose
                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter(), jsonSerializerSettings },
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
                };

                return JsonSerializer.Deserialize<SaxoPortfolio>(File.ReadAllText(filepath), options);
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                return null;
            }
        }
    }

    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        private readonly string Format;
        public CustomDateTimeConverter(string format)
        {
            Format = format;
        }
        public override void Write(Utf8JsonWriter writer, DateTime date, JsonSerializerOptions options)
        {
            writer.WriteStringValue(date.ToString(Format));
        }
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTime.ParseExact(reader.GetString(), Format, null);
        }
    }

}
