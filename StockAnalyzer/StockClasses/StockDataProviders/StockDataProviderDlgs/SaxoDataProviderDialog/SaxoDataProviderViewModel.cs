using Newtonsoft.Json;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    class SaxoDataProviderViewModel : NotifyPropertyChangedBase
    {
        public SaxoDataProviderViewModel(StockDictionary stockDico, string cfgFile, long? saxoId)
        {
            try
            {
                this.configFile = cfgFile;
                var jsonData = HttpGetFromSaxo("https://fr-be.structured-products.saxo/page-api/products/BE/activeProducts?locale=fr_BE");
                // "https://fr-be.structured-products.saxo/page-api/search/*?productsSize=10&underlyingsSize=700&locale=fr_BE");

                if (!string.IsNullOrEmpty(jsonData))
                {
                    var result = JsonConvert.DeserializeObject<UnderlyingRoot>(jsonData);
                    var underlyings = result?.data?.filters?.firstLevel?.underlying?.list;
                    this.Underlyings = underlyings.Values.ToList();
                    // Load config file
                    List<string> underlyingFile = File.Exists(SaxoIntradayDataProvider.SaxoUnderlyingFile) ? File.ReadAllLines(SaxoIntradayDataProvider.SaxoUnderlyingFile).ToList() : new List<string>();
                    var ids = underlyingFile.Select(l => long.Parse(l.Split(',')[0])).ToList();

                    var newIds = this.Underlyings.Where(u => !ids.Contains(u.value)).Select(u => u.value + "," + u.label + ",").ToList();
                    if (newIds.Count > 0)
                    {
                        foreach (var newId in newIds)
                        {
                            var stockName = newId.Split(',')[1].ToUpper();
                            if (stockDico.ContainsKey(stockName))
                            {
                                underlyingFile.Add(newId + stockName);
                            }
                            else
                            {
                                underlyingFile.Add(newId);
                            }
                        }

                        File.WriteAllLines(SaxoIntradayDataProvider.SaxoUnderlyingFile, underlyingFile);

                        MessageBox.Show("New Uderlying detected: " + Environment.NewLine + newIds.Aggregate((i, j) => i + Environment.NewLine + j));
                    }
                }
                this.Entries = new ObservableCollection<SaxoConfigEntry>(SaxoConfigEntry.LoadFromFile(cfgFile));

                if (saxoId != null)
                {
                    this.Underlying = this.Underlyings.FirstOrDefault(u => u.value == saxoId);
                    this.UnderlyingChanged();
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
        }

        private List<SearchUnderlying> underlyings;
        public List<SearchUnderlying> Underlyings { get => underlyings; set => SetProperty(ref underlyings, value); }

        private SearchUnderlying underlying;
        public SearchUnderlying Underlying { get => underlying; set => SetProperty(ref underlying, value); }

        private List<SaxoProduct> products;
        public List<SaxoProduct> Products { get => products; set => SetProperty(ref products, value); }

        private ObservableCollection<SaxoConfigEntry> entries;
        public ObservableCollection<SaxoConfigEntry> Entries { get => entries; set => SetProperty(ref entries, value); }

        public void UnderlyingChanged()
        {
            if (this.underlying == null)
                return;

            var newProducts = new List<SaxoProduct>();
            try
            {
                var jsonData = HttpGetFromSaxo($"https://fr-be.structured-products.saxo/page-api/products/BE/activeProducts?rowsPerPage=1000&underlying={this.underlying.value}&locale=fr_BE");
                //var jsonData = HttpGetFromSaxo($"https://fr-be.structured-products.saxo/page-api/products/BE/list?page=1&rowsPerPage=1000&underlying={entry.key}&locale=fr_BE");
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var result = JsonConvert.DeserializeObject<SaxoResult>(jsonData);
                    foreach (var p in result?.data?.groups?.products)
                    {
                        double parsed = 0.0;
                        double leverage = 0.0;
                        if (double.TryParse(p.leverage.value, out parsed))
                        {
                            if (minLeverage != 0 && maxLeverage != 0)
                            {
                                if (parsed < minLeverage || parsed > maxLeverage)
                                    continue;
                            }
                            leverage = parsed;
                        }
                        double? bid = null;
                        if (p.bid.value != null && double.TryParse(p.bid.value.value, out parsed))
                        {
                            if (minBid != 0 && maxBid != 0)
                            {
                                if (parsed < minBid || parsed > maxBid)
                                    continue;
                            }
                            bid = parsed;
                        }

                        var product = new SaxoProduct
                        {
                            ISIN = p.isin.value,
                            StockName = p.name.value,
                            Type = p.type.value,
                            Ratio = p.ratioCalculated.value,
                            Leverage = leverage,
                            Bid = bid
                        };
                        if (p.ask.value != null && double.TryParse(p.ask.value.value, out parsed))
                        {
                            product.Ask = parsed;
                        }
                        if (product.StockName.Contains("long"))
                        {
                            product.Type += " Long";
                        }
                        else
                        {
                            product.Type += " Short";
                        }
                        newProducts.Add(product);
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            this.Products = newProducts;
        }

        readonly string configFile;

        #region WebHelper


        static private HttpClient httpClient = null;
        private static string HttpGetFromSaxo(string url)
        {
            try
            {
                if (httpClient == null)
                {
                    var handler = new HttpClientHandler();
                    handler.AutomaticDecompression = ~DecompressionMethods.None;

                    httpClient = new HttpClient(handler);
                }
                using var request = new HttpRequestMessage();
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(url);
                var response = httpClient.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    StockLog.Write("StatusCode: " + response.StatusCode + Environment.NewLine + response);
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;

        }
        #endregion



        private CommandBase addCommand;

        public ICommand AddCommand
        {
            get
            {
                addCommand ??= new CommandBase(AddEntry);

                return addCommand;
            }
        }

        private void AddEntry()
        {
            if (this.selectedProduct == null)
                return;
            if (this.Entries.Any(e => e.ISIN == this.selectedProduct.ISIN))
                return;
            var stockName = this.selectedProduct.StockName;
            stockName = stockName.Replace("  ", " ");
            this.Entries.Insert(0, new SaxoConfigEntry { ISIN = this.selectedProduct.ISIN, StockName = stockName });
        }


        private CommandBase saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                saveCommand ??= new CommandBase(Save);
                return saveCommand;
            }
        }

        private void Save()
        {
            try
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                SaxoConfigEntry.SaveToFile(this.Entries, this.configFile);
                Task.Delay(250).Wait();
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
        }

        private SaxoProduct selectedProduct;
        public SaxoProduct SelectedProduct { get => selectedProduct; set => SetProperty(ref selectedProduct, value); }

        private double minLeverage = 2;
        public double MinLeverage { get => minLeverage; set => SetProperty(ref minLeverage, value); }

        private double maxLeverage = 6;
        public double MaxLeverage { get => maxLeverage; set => SetProperty(ref maxLeverage, value); }

        private double minBid;
        public double MinBid { get => minBid; set => SetProperty(ref minBid, value); }

        private double maxBid;
        public double MaxBid { get => maxBid; set => SetProperty(ref maxBid, value); }

        private CommandBase searchCommand;
        public ICommand SearchCommand => searchCommand ??= new CommandBase(Search);

        private void Search()
        {
            this.UnderlyingChanged();
        }
    }
}
