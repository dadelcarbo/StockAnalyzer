using System;
using System.IO;
using System.Linq;
using System.Net;
using StockAnalyzer.StockLogging;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;
using StockAnalyzer.StockMath;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class FINRADataProvider : StockDataProviderBase
    {
        static private string FOLDER = DAILY_SUBFOLDER + @"\FINRA";
        static private string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\FINRA";

        // IStockDataProvider Implementation
        public override bool SupportsIntradayDownload
        {
            get { return false; }
        }

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            // Create data folder if not existing
            if (!Directory.Exists(rootFolder + FOLDER))
            {
                Directory.CreateDirectory(rootFolder + FOLDER);
            }
            string[] names = new string[] { "BOND_AD.ALL", "BOND_AD.HY", "BOND_AD.IG", "McClellan.HY", "McClellan.IG" };
            StockSerie stockSerie = null;

            if (finraSerie == null)
            {
                finraSerie = LoadFINRASerie(rootFolder + FOLDER + "\\FINRA_Bond_Breadth.xml");
            }

            foreach (string name in names)
            {
                if (!stockDictionary.ContainsKey(name))
                {
                    stockSerie = new StockSerie(name, name, StockSerie.Groups.BREADTH, StockDataProvider.FINRA);
                    stockDictionary.Add(name, stockSerie);
                }
            }
            if (download)
            {
                this.DownloadDailyData(rootFolder);
            }
        }

        static StockFINRASerie finraSerie = null;

        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            if (finraSerie == null)
            {
                finraSerie = LoadFINRASerie(rootFolder + FOLDER + "\\FINRA_Bond_Breadth.xml");
            }

            // Convert from StockFINRASerie to StockSerie
            var fields = stockSerie.StockName.Split('.');
            StockDailyValue dailyValue = null;
            switch (fields[0])
            {
                case "BOND_AD":
                    switch (fields[1])
                    {
                        case "ALL":

                            break;
                        case "IG":
                            foreach (var val in finraSerie.InvestGrade)
                            {
                                float open = -1.0f + (float)val.Advances * 2.0f / (float)val.Total;
                                dailyValue = new StockDailyValue(stockSerie.StockName, open, open, open, open, val.Volume, val.Date);
                                stockSerie.Add(val.Date, dailyValue);
                            }
                            break;
                        case "HY":
                            foreach (var val in finraSerie.HighYield)
                            {
                                float open = -1.0f + (float)val.Advances * 2.0f / (float)val.Total;
                                dailyValue = new StockDailyValue(stockSerie.StockName, open, open, open, open, val.Volume, val.Date);
                                stockSerie.Add(val.Date, dailyValue);
                            }
                            break;
                    }
                    break;
                case "McClellan":
                    switch (fields[1])
                    {
                        case "ALL":

                            break;
                        case "IG":
                            {
                                var serie = StockDictionary.StockDictionarySingleton["BOND_AD.IG"];
                                if (!serie.Initialise()) return false;
                                FloatSerie ema19 = serie.GetIndicator("EMA(19)").Series[0];
                                FloatSerie ema39 = serie.GetIndicator("EMA(39)").Series[0];
                                var diff = ema19 - ema39;
                                int i = 0;
                                foreach (var val in serie.Values)
                                {
                                    float open = diff[i++];
                                    dailyValue = new StockDailyValue(stockSerie.StockName, open, open, open, open, val.VOLUME, val.DATE);
                                    stockSerie.Add(val.DATE, dailyValue);
                                }
                            }
                            break;
                        case "HY":
                            {
                                var serie = StockDictionary.StockDictionarySingleton["BOND_AD.HY"];
                                if (!serie.Initialise()) return false;
                                FloatSerie ema19 = serie.GetIndicator("EMA(19)").Series[0];
                                FloatSerie ema39 = serie.GetIndicator("EMA(39)").Series[0];
                                var diff = ema19 - ema39;
                                int i = 0;
                                foreach (var val in serie.Values)
                                {
                                    float open = diff[i++];
                                    dailyValue = new StockDailyValue(stockSerie.StockName, open, open, open, open, val.VOLUME, val.DATE);
                                    stockSerie.Add(val.DATE, dailyValue);
                                }
                            }
                            break;
                    }
                    break;
            }

            return true;
        }

        StockFINRASerie LoadFINRASerie(string fileName)
        {
            if (File.Exists(fileName))
            {
                XmlSerializer xmlSerialier = new XmlSerializer(typeof(StockFINRASerie));
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    return xmlSerialier.Deserialize(fs) as StockFINRASerie;
                }
            }
            return new StockFINRASerie();
        }

        public bool DownloadDailyData(string rootFolder)
        {
            bool res = false;
            // http://finra-markets.morningstar.com/transferPage.jsp?path=http%3A%2F%2Fmuni-internal.morningstar.com%2Fpublic%2FMarketBreadth%2FC&date=11%2F14%2F2006&_=1480663564792
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                XmlSerializer xs = new XmlSerializer(typeof(StockFINRASerie));
                var sp500 = StockDictionary.StockDictionarySingleton["SP500"];
                sp500.Initialise();
                var dates = sp500.Keys;

                bool isUpTodate = false;
                DateTime lastDate;
                if (finraSerie.HighYield.Count > 0)
                {
                    // This serie already exist, download just the missing data.
                    lastDate = finraSerie.HighYield.Last().Date;
                }
                else
                {
                    lastDate = DateTime.MinValue;
                }
                if (lastDate == dates.Last().Date) return true;
                DateTime previousDate = dates.First(d => d.Date > lastDate);
                foreach (DateTime date in dates.Where(d => d.Date > lastDate))
                {
                    if (previousDate == null)
                    {
                        previousDate = date;
                    }
                    NotifyProgress("Loading FINRA Data for " + date.ToShortDateString());
                    StockAdvDecl investGrade, highYield;
                    if (this.DowloadFromFINRA(date, out investGrade, out highYield))
                    {
                        finraSerie.HighYield.Add(highYield);
                        finraSerie.InvestGrade.Add(investGrade);
                        if (previousDate.Year != date.Year)
                        {
                            // Save File
                            using (FileStream fs = new FileStream(rootFolder + FOLDER + "\\FINRA_Bond_Breadth.xml", FileMode.Create))
                            {
                                xs.Serialize(fs, finraSerie);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error Downloading data");
                    }
                    previousDate = date;
                }
                // Save File
                using (FileStream fs = new FileStream(rootFolder + FOLDER + "\\FINRA_Bond_Breadth.xml", FileMode.Create))
                {
                    xs.Serialize(fs, finraSerie);
                }
                res = true;
            }
            return res;
        }

        private static List<Cookie> cookies = null;
        private static List<Cookie> Cookies
        {
            get
            {
                if (cookies == null)
                {
                    InitFINRACookie();
                }
                return cookies;
            }
        }
        static HttpWebResponse response = null;
        private static void InitFINRACookie()
        {
            //string url = "http://finra-markets.morningstar.com/BondCenter/TRACEMarketAggregateStats.jsp";
            string url = "http://finra-markets.morningstar.com/finralogin.jsp";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.CookieContainer = new CookieContainer();
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "Get";
            req.AllowAutoRedirect = true;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add("Accept-Language", "fr,fr-fr;q=0.8,en-us;q=0.5,en;q=0.3");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36";
            req.Referer = url;
            req.KeepAlive = true;

            //DumpHeader("Main Req", req.Headers);
            //DumpCookies(req.CookieContainer);

            bool success = false;
            int tries = 3;
            while (!success && tries > 0)
            {
                tries--;
                try
                {
                    response = (HttpWebResponse)req.GetResponse();
                    {
                        // Get the stream containing content returned by the server.
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(dataStream))
                            {
                                //DumpHeader("Main Response", response.Headers);
                                //DumpCookies(response.Cookies);

                                success = true;
                                var cookieString = response.Headers[HttpResponseHeader.SetCookie].Split(';');

                                var fields = cookieString[0].Split('=');
                                cookies = new List<Cookie>();
                                cookies.Add(new Cookie(fields[0], fields[1]) { Path = "/", Domain = ".morningstar.com" });
                                cookies.Add(new Cookie("SessionID", fields[1]) { Path = "/", Domain = ".morningstar.com" });
                                cookies.Add(new Cookie("Instid", "FINRA") { Path = "/", Domain = ".morningstar.com" });
                                cookies.Add(new Cookie("UsrID", "41151") { Path = "/", Domain = ".morningstar.com" });
                                cookies.Add(new Cookie("UsrName", "FINRA.QSAPIDEF@morningstar.com") { Path = "/", Domain = ".morningstar.com" });

                                //foreach (var cookieString in cookie.Split(';'))
                                //{
                                //    var fields = cookieString.Trim().Split('=');
                                //    if (fields[0].ToLower().Contains("domain")) continue;
                                //    cookies.Add(new Cookie(fields[0], fields[1]));
                                //}
                            }
                        }
                    }
                }
                catch (Exception) { }
            }
        }

        private bool DowloadFromFINRA(DateTime date, out StockAdvDecl investGrade, out StockAdvDecl highYield)
        {
            bool res = false;
            investGrade = null;
            highYield = null;

            StockWeb.StockWebHelper wh = new StockWeb.StockWebHelper();
            string url = "http://finra-markets.morningstar.com/transferPage.jsp?path=http%3A%2F%2Fmuni-internal.morningstar.com%2Fpublic%2FMarketBreadth%2FC&date=$MM%2F$DD%2F$YYYY";
            url = url.Replace("$YYYY", date.Year.ToString());
            url = url.Replace("$DD", date.Day.ToString());
            url = url.Replace("$MM", date.Month.ToString());

            //Console.WriteLine(url);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.CookieContainer = new CookieContainer();

            foreach (Cookie cookie in Cookies)
            {
                req.CookieContainer.Add(cookie);
            }

            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "Get";
            req.AllowAutoRedirect = true;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add("Accept-Language", "en-US,en;q=0.8,fr;q=0.6");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36";
            req.Referer = url;

            //DumpHeader("Inner Req", req.Headers);
            //DumpCookies(req.CookieContainer);

            bool success = false;
            int tries = 3;
            while (!success && tries > 0)
            {
                tries--;
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
                    {
                        //DumpHeader("Inner Response", response.Headers);

                        // Get the stream containing content returned by the server.
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(dataStream))
                            {
                                string html = reader.ReadToEnd();
                                if (string.IsNullOrWhiteSpace(html)) return false;

                                return ParseFINRAHtml(html, date, out investGrade, out highYield);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            return false;
        }

        private static void DumpCookies(CookieContainer cookieContainer)
        {
            foreach (var cookie in cookieContainer.GetCookies(new Uri("http://finra-markets.morningstar.com")))
            {
                Console.WriteLine(cookie.ToString());
            }
        }
        private static void DumpCookies(CookieCollection cookies)
        {
            foreach (var cookie in cookies)
            {
                Console.WriteLine(cookie.ToString());
            }
        }
        private static void DumpHeader(string text, WebHeaderCollection headers)
        {
            Console.WriteLine();
            Console.WriteLine(text);
            foreach (var header in headers)
            {
                Console.WriteLine(header.ToString() + ":" + headers[header.ToString()].ToString());
            }
        }


        // Private methods
        private bool ParseFINRAHtml(string html, DateTime date, out StockAdvDecl investGrade, out StockAdvDecl highYield)
        {
            try
            {
                WebBrowser browser = new WebBrowser();
                browser.ScriptErrorsSuppressed = true;
                browser.DocumentText = html;
                browser.Document.OpenNew(true);
                browser.Document.Write(html);
                browser.Refresh();


                var table = browser.Document.GetElementsByTagName("table").Cast<HtmlElement>().First();
                var data = getTableData(table);

                investGrade = new StockAdvDecl()
                {
                    Date = date,
                    Advances = int.Parse(data[2][2]),
                    Declines = int.Parse(data[3][2]),
                    Unchanged = int.Parse(data[4][2]),
                    Volume = data[7][2] != null ? int.Parse(data[7][2]) : 0
                };
                highYield = new StockAdvDecl()
                {
                    Date = date,
                    Advances = int.Parse(data[2][3]),
                    Declines = int.Parse(data[3][3]),
                    Unchanged = int.Parse(data[4][3]),
                    Volume = data[7][3] != null ? int.Parse(data[7][3]) : 0
                };
                return true;
            }
            catch (Exception)
            {
                investGrade = null;
                highYield = null;
                return false;
            }

        }
        static private List<List<string>> getTableData(HtmlElement tbl)
        {
            List<List<string>> data = new List<List<string>>();

            HtmlElementCollection rows = tbl.GetElementsByTagName("tr");
            HtmlElementCollection cols; // = rows.GetElementsByTagName("th");
            foreach (HtmlElement tr in rows)
            {
                List<string> row = new List<string>();
                cols = tr.GetElementsByTagName("th");
                foreach (HtmlElement td in cols)
                {
                    row.Add(WebUtility.HtmlDecode(td.InnerText));
                }
                cols = tr.GetElementsByTagName("td");
                foreach (HtmlElement td in cols)
                {
                    row.Add(WebUtility.HtmlDecode(td.InnerText));
                }
                if (row.Count > 0) data.Add(row);
            }

            return data;
        }

        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            throw new NotImplementedException();
        }
    }
    public class StockAdvDecl
    {
        public StockAdvDecl()
        {
        }

        public DateTime Date { get; set; }
        public int Advances { get; set; }
        public int Declines { get; set; }
        public int Unchanged { get; set; }
        [XmlIgnore]
        public int Total { get { return Advances + Declines + Unchanged; } }
        public int Volume { get; set; }
    }
    public class StockFINRASerie
    {
        public StockFINRASerie()
        {
            this.InvestGrade = new List<StockAdvDecl>();
            this.HighYield = new List<StockAdvDecl>();
        }
        public List<StockAdvDecl> InvestGrade { get; set; }
        public List<StockAdvDecl> HighYield { get; set; }
    }
}
