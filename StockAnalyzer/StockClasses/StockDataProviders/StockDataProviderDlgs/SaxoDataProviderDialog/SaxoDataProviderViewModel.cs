using Newtonsoft.Json;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog
{
    class SaxoDataProviderViewModel : NotifyPropertyChangedBase
    {
        public SaxoDataProviderViewModel(StockDictionary stockDico, string cfgFile)
        {
            this.configFile = cfgFile;
            var jsonData = HttpGetFromSaxo("https://fr-be.structured-products.saxo/page-api/search/*?productsSize=1&underlyingsSize=700&locale=fr_BE");
            if (!string.IsNullOrEmpty(jsonData))
            {
                var result = JsonConvert.DeserializeObject<SaxoUnderlyings>(jsonData);
                this.Underlyings = result?.entries?.FirstOrDefault(e => e.key == "underlyings")?.entries;
            }
            this.Entries = new ObservableCollection<SaxoConfigEntry>(SaxoConfigEntry.LoadFromFile(cfgFile));
        }

        private List<Entry> underlyings;
        public List<Entry> Underlyings { get => underlyings; set => SetProperty(ref underlyings, value); }

        private List<SaxoProduct> products;
        public List<SaxoProduct> Products { get => products; set => SetProperty(ref products, value); }

        private ObservableCollection<SaxoConfigEntry> entries;
        public ObservableCollection<SaxoConfigEntry> Entries { get => entries; set => SetProperty(ref entries, value); }

        Entry previousEntry;
        public void UnderlyingChanged(Entry entry)
        {
            if (entry == previousEntry)
                return;
            var newProducts = new List<SaxoProduct>();
            try
            {
                var jsonData = HttpGetFromSaxo($"https://fr-be.structured-products.saxo/page-api/products/BE/list?page=1&rowsPerPage=1000&underlying={entry.key}&locale=fr_BE");
                if (!string.IsNullOrEmpty(jsonData))
                {
                    var result = JsonConvert.DeserializeObject<SaxoResult>(jsonData);
                    foreach (var p in result?.data?.groups?.products)
                    {
                        double parsed;
                        var product = new SaxoProduct
                        {
                            ISIN = p.isin.value,
                            StockName = p.name.value,
                            Type = p.type.value,
                            Ratio = p.ratioCalculated.value
                        };
                        if (product.StockName.Contains("long"))
                        {
                            product.Type += " Long";
                        }
                        else
                        {
                            product.Type += " Short";
                        }
                        if (double.TryParse(p.ask.value.value, out parsed))
                        {
                            product.Ask = parsed;
                        }
                        if (double.TryParse(p.bid.value.value, out parsed))
                        {
                            product.Bid = parsed;
                        }
                        if (double.TryParse(p.leverage.value, out parsed))
                        {
                            product.Leverage = parsed;
                        }
                        newProducts.Add(product);
                    }
                    previousEntry = entry;
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            this.Products = newProducts;
        }

        string configFile;
        StockDictionary dictionary;

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
                using (var request = new HttpRequestMessage())
                {
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
                if (addCommand == null)
                {
                    addCommand = new CommandBase(AddEntry);
                }

                return addCommand;
            }
        }

        private void AddEntry()
        {
            if (this.selectedProduct == null)
                return;
            if (this.Entries.Any(e => e.ISIN == this.selectedProduct.ISIN))
                return;
            var stockName = "TURBO_" + this.selectedProduct.StockName.Split(' ')[0].ToUpper() + " " + (this.selectedProduct.Type.ToLower().Contains("short") ? "SHORT" : "LONG");
            this.Entries.Insert(0, new SaxoConfigEntry { ISIN = this.selectedProduct.ISIN, StockName = stockName });
        }


        private CommandBase saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new CommandBase(Save);
                }
                return saveCommand;
            }
        }

        private void Save()
        {
            SaxoConfigEntry.SaveToFile(this.Entries, this.configFile);
        }

        private SaxoProduct selectedProduct;
        public SaxoProduct SelectedProduct { get => selectedProduct; set => SetProperty(ref selectedProduct, value); }
    }
    public class SaxoConfigEntry
    {
        public SaxoConfigEntry()
        {
        }
        public string ISIN { get; set; }
        public string StockName { get; set; }

        public static IEnumerable<SaxoConfigEntry> LoadFromFile(string fileName)
        {
            string line;
            var entries = new List<SaxoConfigEntry>();
            if (File.Exists(fileName))
            {
                using (var sr = new StreamReader(fileName, true))
                {
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                        var row = line.Split(',');

                        entries.Add(new SaxoConfigEntry() // 8894,CC,FUT_COM_COCOA,FUTURE
                        {
                            ISIN = row[0],
                            StockName = row[1]
                        });
                    }
                }
            }
            return entries;
        }

        public static void SaveToFile(IList<SaxoConfigEntry> entries, string fileName)
        {
            using (var sr = new StreamWriter(fileName, false))
            {
                foreach (var entry in entries.OrderBy(e => e.StockName))
                {
                    sr.WriteLine(
                        entry.ISIN + "," +
                        entry.StockName
                        );
                }
            }
        }
    }

}
