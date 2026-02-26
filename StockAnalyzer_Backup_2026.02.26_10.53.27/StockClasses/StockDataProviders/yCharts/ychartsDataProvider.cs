using Newtonsoft.Json;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockClasses.StockDataProviders.yCharts;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class ychartsDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\ycharts";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\ycharts";
        static private readonly string CONFIG_FILE = "ychartsDownload.cfg";
        static public string UserConfigFileName => CONFIG_FILE;

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            // Create data folder if not existing
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + INTRADAY_FOLDER);
            }

            // Parse ychartsDownload.cfg file
            this.needDownload = download;
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE));

            var stockName = "CBOE_EPCR";
            var stockSymbol = "I:CBOEEPCR";
            var stockSerie = new StockSerie(stockName, stockSymbol, StockSerie.Groups.INDICATOR, StockDataProvider.yCharts, BarDuration.Daily);
            stockDictionary.Add(stockName, stockSerie);
        }

        public override bool SupportsIntradayDownload => true;

        public override bool LoadData(StockSerie stockSerie)
        {
            var archiveFileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, $"{stockSerie.StockName}.txt");

            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
            }
            return stockSerie.Count > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="period">1D - 1 minute bars from begining of the current day<br/>
        /// 2D - 5 minutes bars from the last 24 Hours<br/>
        /// 1W - 1 hour bar for 1 week period</param>
        /// <returns></returns>
        public string FormatIntradayURL(string ticker, string period)
        {
            return $"https://ycharts.com/charts/fund_data.json?calcs=&chartId=&chartType=interactive&correlations=&customGrowthAmount=&dataInLegend=value&dateSelection=range&displayDateRange=false&endDate=&format=real&legendOnChart=false&lineAnnotations=&nameInLegend=name_and_ticker&note=&partner=basic_2000&performanceDisclosure=false&quoteLegend=false&recessions=false&scaleType=linear&securities=id%3AI%3ACBOEEPCR%2Cinclude%3Atrue%2C%2C&securityGroup=&securitylistName=&securitylistSecurityId=&source=false&splitType=single&startDate=&title=&units=false&useCustomColors=false&useEstimates=false&zoom=1&hideValueFlags=false&redesign=true&chartAnnotations=&axisExtremes=&sortColumn=&sortDirection=&le_o=quote_page_fund_data&maxPoints=739&chartCreator=false";
        }

        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            return false;
        }

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for " + stockSerie.StockName);

                using var wc = new WebClient();
                wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                try
                {
                    string url = FormatIntradayURL(stockSerie.ISIN, "1W");

                    var jsonData = ychartsDataProvider.HttpGetFromSaxo(url);
                    if (string.IsNullOrEmpty(jsonData))
                        return false;
                    var saxoData = JsonConvert.DeserializeObject<ychartJson>(jsonData, Converter.Settings);
                    if (saxoData?.chart_data?.Length == 0)
                        return false;

                    stockSerie.IsInitialised = false;
                    this.LoadData(stockSerie);
                    DateTime lastDate = DateTime.MinValue;
                    if (stockSerie.Count > 0)
                    {
                        lastDate = stockSerie.Keys.Last();
                    }

                    int nbNewBars = 0;
                    foreach (var bar in saxoData.chart_data[0][0].raw_data)
                    {
                        var date = refDate.AddMilliseconds((double)bar[0]);
                        if (date > lastDate)
                        {
                            var newBar = new StockDailyValue((float)bar[1], (float)bar[1], (float)bar[1], (float)bar[1], 0, date);
                            stockSerie.Add(newBar.DATE, newBar);
                            nbNewBars++;
                        }
                    }

                    if (nbNewBars > 0)
                    {
                        var archiveFileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, $"{stockSerie.StockName}.txt");
                        stockSerie.SaveToCSVFromDateToDate(archiveFileName, DateTime.MinValue, stockSerie.Keys.Last());
                    }
                    return true;
                }
                catch (Exception e)
                {
                    StockLog.Write(e);
                }
            }
            return false;
        }


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
        private void InitFromFile(StockDictionary stockDictionary, bool download, string fileName)
        {
            string line;
            if (File.Exists(fileName))
            {
                using var sr = new StreamReader(fileName, true);
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    if (line.StartsWith("$")) break;

                    var row = line.Split(',');
                    if (!stockDictionary.ContainsKey(row[1]))
                    {
                        var stockSerie = new StockSerie(row[1], row[0], StockSerie.Groups.INDICATOR, StockDataProvider.yCharts, BarDuration.Daily);
                        stockSerie.ISIN = row[0];
                        stockDictionary.Add(row[1], stockSerie);
                        if (row.Length == 3)
                        {
                            stockSerie.Uic = long.Parse(row[2]);
                        }

                        if (RefSerie == null && download) // Check if provider is up to date by checking the reference serie
                        {
                            // Check if download needed.
                            stockSerie.Initialise();
                            DateTime refDate = DateTime.MinValue;
                            if (stockSerie.Count > 0)
                            {
                                refDate = stockSerie.Keys.Last();
                            }
                            this.DownloadIntradayData(stockSerie);
                            if (stockSerie.Initialise())
                            {
                                needDownload = refDate < stockSerie.Keys.Last();
                                RefSerie = stockSerie;
                            }
                        }
                        else
                        {
                            if (download && this.needDownload)
                            {
                                this.DownloadIntradayData(stockSerie);
                            }
                        }
                    }
                    else
                    {
                        StockLog.Write("ychart Entry: " + row[1] + " already in stockDictionary");
                    }
                }
            }
        }
        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            return DialogResult.OK;
        }

        public override string DisplayName => "yCharts";

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            Process.Start($"https://ycharts.com/");
        }

        public override bool RemoveEntry(StockSerie stockSerie)
        {
            return false;
        }

        public override void ApplyTrimBefore(StockSerie stockSerie, DateTime date)
        {
            return;
        }
    }
}
