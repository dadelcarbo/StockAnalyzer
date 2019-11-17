using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class InvestingDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private string FOLDER = @"\data\daily\Investing";
        static private string ARCHIVE_FOLDER = @"\data\archive\daily\Investing";

        static private string CONFIG_FILE = @"\InvestingDownload.cfg";
        static private string CONFIG_FILE_USER = @"\InvestingDownload.user.cfg";
        public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

        //For($i = 1; $i - le 16; $i++) {
        //    $Response = Invoke - WebRequest - Uri "https://www.investing.com/stock-screener/Service/SearchStocks" - Method "POST" - Headers @{ "Cookie" = "adBlockerNewUserDomains=1469044276; r_p_s=1; optimizelyEndUserId=oeu1469044277264r0.7416594930578495; _ga=GA1.2.1633392168.1469044281; G_ENABLED_IDPS=google; _hjDonePolls=287233; optimizelyBuckets=%7B%227141820025%22%3A%227141620081%22%2C%227566682463%22%3A%227618750148%22%2C%228229907422%22%3A%228235722598%22%2C%229398500706%22%3A%229257037455%22%2C%2210555684981%22%3A%2210586870005%22%2C%2210474386415%22%3A%2210562174079%22%2C%227472480192%22%3A%220%22%2C%228176331464%22%3A%220%22%2C%228465822231%22%3A%220%22%7D; __gads=ID=7281042aeb6494b9:T=1527740810:S=ALNI_MbgSGKvySwda-9HQHQtrKKBosdDBw; cookieConsent=was-set; r_p_s_n=1; _VT_content_200344267_1=1; __qca=P0-1169702415-1537296311846; PHPSESSID=acofolvn3scinbf00ig6sat1u4; comment_notification_101156590=1; StickySession=id.92382590183.971www.investing.com; _gid=GA1.2.941308646.1537603549; gtmFired=OK; geoC=FR; SideBlockUser=a%3A2%3A%7Bs%3A10%3A%22stack_size%22%3Ba%3A1%3A%7Bs%3A11%3A%22last_quotes%22%3Bi%3A8%3B%7Ds%3A6%3A%22stacks%22%3Ba%3A1%3A%7Bs%3A11%3A%22last_quotes%22%3Ba%3A7%3A%7Bi%3A0%3Ba%3A3%3A%7Bs%3A7%3A%22pair_ID%22%3Bs%3A4%3A%228836%22%3Bs%3A10%3A%22pair_title%22%3Bs%3A0%3A%22%22%3Bs%3A9%3A%22pair_link%22%3Bs%3A19%3A%22%2Fcommodities%2Fsilver%22%3B%7Di%3A1%3Ba%3A3%3A%7Bs%3A7%3A%22pair_ID%22%3Bs%3A4%3A%228869%22%3Bs%3A10%3A%22pair_title%22%3Bs%3A0%3A%22%22%3Bs%3A9%3A%22pair_link%22%3Bs%3A26%3A%22%2Fcommodities%2Fus-sugar-no11%22%3B%7Di%3A2%3Ba%3A3%3A%7Bs%3A7%3A%22pair_ID%22%3Bs%3A4%3A%228832%22%3Bs%3A10%3A%22pair_title%22%3Bs%3A0%3A%22%22%3Bs%3A9%3A%22pair_link%22%3Bs%3A24%3A%22%2Fcommodities%2Fus-coffee-c%22%3B%7Di%3A3%3Ba%3A3%3A%7Bs%3A7%3A%22pair_ID%22%3Bs%3A3%3A%22386%22%3Bs%3A10%3A%22pair_title%22%3Bs%3A0%3A%22%22%3Bs%3A9%3A%22pair_link%22%3Bs%3A15%3A%22%2Fequities%2Faccor%22%3B%7Di%3A4%3Ba%3A3%3A%7Bs%3A7%3A%22pair_ID%22%3Bs%3A6%3A%22961656%22%3Bs%3A10%3A%22pair_title%22%3Bs%3A0%3A%22%22%3Bs%3A9%3A%22pair_link%22%3Bs%3A40%3A%22%2Fequities%2Fheckler---koch-beteiligungs-ag%22%3B%7Di%3A5%3Ba%3A3%3A%7Bs%3A7%3A%22pair_ID%22%3Bs%3A6%3A%22989584%22%3Bs%3A10%3A%22pair_title%22%3Bs%3A0%3A%22%22%3Bs%3A9%3A%22pair_link%22%3Bs%3A35%3A%22%2Fequities%2Fgreen-energy-4-seasons-sa%22%3B%7Di%3A6%3Ba%3A3%3A%7Bs%3A7%3A%22pair_ID%22%3Bs%3A5%3A%2230215%22%3Bs%3A10%3A%22pair_title%22%3Bs%3A0%3A%22%22%3Bs%3A9%3A%22pair_link%22%3Bs%3A29%3A%22%2Fequities%2Fwal-mart-stores-inc%22%3B%7D%7D%7D%7D; _hjIncludedInSample=1; optimizelySegments=%7B%224225444387%22%3A%22gc%22%2C%224226973206%22%3A%22search%22%2C%224232593061%22%3A%22false%22%2C%225010352657%22%3A%22none%22%7D; nyxDorf=NDMzMTFnYz5mOm82bj5hZmc8P2ZmNzMxPT00PTY0Mjk2YjBmNTkyMGZiOTEzPGNnM2cwZmVmNDQxOGZsMGRuOjRjM2oxNmM7ZjJvMg%3D%3D; billboardCounter_1=1; ses_id=OHZiIzM8MTlhJWttYjA0NjBgMWtiYGFlZ2NlbzI1Z3FkcDQ6MWY3cT4xO3VnZGR4YzZjY2UwNTNnYjBpYG1kZzg%2FYjgzYTFrYWFrZ2JlND4wYjFvYmFhNWdvZTIyMmc8ZDc0YjFkNzY%2BPTtlZ25kbGNxY39lITUkZzUwYGAhZCM4N2IjM2AxP2Fma2FiNjQwMDUxPGJsYWBnNGViMjZnf2Qv"; "Origin" = "https://www.investing.com"; "Accept-Encoding" = "gzip, deflate, br"; "Accept-Language" = "en-US,en;q=0.9,fr;q=0.8"; "User-Agent" = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.100 Safari/537.36"; "Accept" = "application/json, text/javascript, */*; q=0.01"; "Referer" = "https://www.investing.com/stock-screener/?sp=country::29|sector::a|industry::a|equityType::a%3Ceq_market_cap;2"; "X-Requested-With" = "XMLHttpRequest"}
        //                -ContentType "application/x-www-form-urlencoded" - Body "country%5B%5D=22&sector=7%2C5%2C12%2C3%2C8%2C9%2C1%2C6%2C2%2C4%2C10%2C11&industry=81%2C56%2C59%2C41%2C68%2C67%2C88%2C51%2C72%2C47%2C12%2C8%2C50%2C2%2C71%2C9%2C69%2C45%2C46%2C13%2C94%2C102%2C95%2C58%2C100%2C101%2C87%2C31%2C6%2C38%2C79%2C30%2C77%2C28%2C5%2C60%2C18%2C26%2C44%2C35%2C53%2C48%2C49%2C55%2C78%2C7%2C86%2C10%2C1%2C34%2C3%2C11%2C62%2C16%2C24%2C20%2C54%2C33%2C83%2C29%2C76%2C37%2C90%2C85%2C82%2C22%2C14%2C17%2C19%2C43%2C89%2C96%2C57%2C84%2C93%2C27%2C74%2C97%2C4%2C73%2C36%2C42%2C98%2C65%2C70%2C40%2C99%2C39%2C92%2C75%2C66%2C63%2C21%2C25%2C64%2C61%2C32%2C91%2C52%2C23%2C15%2C80&equityType=ORD%2CDRC%2CPreferred%2CUnit%2CClosedEnd%2CREIT%2CELKS%2COpenEnd%2CParticipationShare%2CCapitalSecurity%2CPerpetualCapitalSecurity%2CGuaranteeCertificate%2CIGC%2CWarrant%2CSeniorNote%2CDebenture%2CETF%2CADR%2CETC%2CETN&pn=$i&order%5Bcol%5D=eq_market_cap&order%5Bdir%5D=d"

        //    $Stream = [System.IO.StreamWriter]::new("C:\Users\dadelcarbo\Downloads\content$i.txt", $false)
        //    try
        //    {
        //        $Stream.Write($response.Content)
        //    }
        //    finally
        //    {
        //        $Stream.Dispose()
        //    }
        //}

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            // Parse Investing.cfg file// Create data folder if not existing
            if (!Directory.Exists(rootFolder + FOLDER))
            {
                Directory.CreateDirectory(rootFolder + FOLDER);
            }
            if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
            }

            this.needDownload = download;

            // Parse InvestingDownload.cfg file
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE);
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE_USER);
        }
        private void InitFromFile(string rootFolder, StockDictionary stockDictionary, bool download, string fileName)
        {
            string line;
            if (File.Exists(fileName))
            {
                using (var sr = new StreamReader(fileName, true))
                {
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                        var row = line.Split(',');
                        if (!stockDictionary.ContainsKey(row[2]))
                        {
                            var stockSerie = new StockSerie(row[2], row[1], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[3]), StockDataProvider.Investing);
                            stockSerie.Ticker = long.Parse(row[0]);

                            stockDictionary.Add(row[2], stockSerie);
                            if (download && this.needDownload)
                            {
                                this.needDownload = this.DownloadDailyData(rootFolder, stockSerie);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Investing Daily Entry: " + row[2] + " already in stockDictionary");
                        }
                    }
                }
            }
        }

        public override bool SupportsIntradayDownload => false;

        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            bool res = false;
            var archiveFileName = rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
                res = true;
            }

            var fileName = rootFolder + FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

            if (File.Exists(fileName))
            {
                if (ParseDailyData(stockSerie, fileName))
                {
                    stockSerie.Values.Last().IsComplete = false;
                    var lastDate = stockSerie.Keys.Last();

                    var previousMonth = lastDate.AddMonths(-2);
                    var firstArchiveDate = new DateTime(previousMonth.Year, previousMonth.Month, 1); // Archive two month back

                    stockSerie.SaveToCSVFromDateToDate(archiveFileName, stockSerie.Keys.First(), lastDate);
                    File.Delete(fileName);
                }
                else
                {
                    return false;
                }
                res = true;
            }
            return res;
        }

        static DateTime refDate = new DateTime(1970, 01, 01) + (DateTime.Now - DateTime.UtcNow);
        public string FormatURL(long ticker, DateTime startDate, DateTime endDate)
        {
            var interval = "D";
            var from = (long)((startDate - refDate).TotalSeconds);
            var to = (long)((endDate - refDate).TotalSeconds);
            return $"http://tvc4.forexpros.com/0f8a29a810801b55700d8d096869fe1f/1567000256/1/1/8/history?symbol={ticker}&resolution={interval}&from={from}&to={to}";
        }

        static bool first = true;
        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading daily data for " + stockSerie.StockName);

                var fileName = rootFolder + FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                if (File.Exists(fileName))
                {
                    var lastWriteTime = File.GetLastWriteTime(fileName);
                    if (first && lastWriteTime > DateTime.Now.AddHours(-2)
                       || (DateTime.Today.DayOfWeek == DayOfWeek.Sunday && lastWriteTime.Date >= DateTime.Today.AddDays(-1))
                       || (DateTime.Today.DayOfWeek == DayOfWeek.Saturday && lastWriteTime.Date >= DateTime.Today))
                    {
                        first = false;
                        return false;
                    }
                    else
                    {
                        if (File.GetLastWriteTime(fileName) > DateTime.Now.AddMinutes(-2))
                            return false;
                    }
                }
                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                    var url = string.Empty;
                    if (stockSerie.Initialise() && stockSerie.Count > 0)
                    {
                        var startDate = stockSerie.ValueArray[stockSerie.LastCompleteIndex].DATE.Date.AddDays(-7);
                        if (startDate > DateTime.Today) return true;

                        url = FormatURL(stockSerie.Ticker, startDate, DateTime.Today);
                    }
                    else
                    {
                        url = FormatURL(stockSerie.Ticker, DateTime.Today.AddYears(-5), DateTime.Today);
                    }

                    int nbTries = 3;
                    while (nbTries > 0)
                    {
                        try
                        {
                            wc.DownloadFile(url, fileName);
                            stockSerie.IsInitialised = false;
                            return true;
                        }
                        catch (Exception e)
                        {
                            nbTries--;
                        }
                    }
                }
            }
            return false;
        }

        private static bool ParseDailyData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    var barchartJson = BarChartJSon.FromJson(sr.ReadToEnd());

                    for (var i = 0; i < barchartJson.C.Length; i++)
                    {
                        if (barchartJson.O[i] == 0 && barchartJson.H[i] == 0 && barchartJson.L[i] == 0 && barchartJson.C[i] == 0)
                            continue;

                        var openDate = refDate.AddSeconds(barchartJson.T[i]).Date;
                        if (!stockSerie.ContainsKey(openDate))
                        {
                            var volString = barchartJson.V[i];
                            long vol = 0;
                            long.TryParse(barchartJson.V[i], out vol);
                            var dailyValue = new StockDailyValue(stockSerie.StockName,
                                   barchartJson.O[i],
                                   barchartJson.H[i],
                                   barchartJson.L[i],
                                   barchartJson.C[i],
                                   vol,
                                   openDate);

                            stockSerie.Add(dailyValue.DATE, dailyValue);
                        }
                    }
                    stockSerie.ClearBarDurationCache();

                    res = true;
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write("Unable to parse daily data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            var configDlg = new InvestingIntradayDataProviderConfigDlg(stockDico, this.UserConfigFileName);
            return configDlg.ShowDialog();
        }

        public string DisplayName => "Investing";
    }
}
