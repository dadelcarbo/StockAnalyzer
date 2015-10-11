using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Windows.Forms;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
   public class ABCDataProvider : StockDataProviderBase, IConfigDialog
   {
      static private string ABC_INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\ABC";
      static private string ABC_DAILY_FOLDER = DAILY_SUBFOLDER + @"\ABC";
      static private string ABC_ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\ABC";
      static private string CONFIG_FILE = @"\EuronextDownload.cfg";
      static private string CONFIG_FILE_USER = @"\EuronextDownload.user.cfg";

      static private string labelViewState = string.Empty;
      static private string labelEventValidation = string.Empty;
      static private string intradayViewState = string.Empty;
      static private string intradayEventValidation = string.Empty;
      static private string dailyViewState = string.Empty;
      static private string dailyEventValidation = string.Empty;

      public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

      private static StockDictionary stockDictionary = null;

      // IStockDataProvider Implementation
      public override bool SupportsIntradayDownload
      {
         get { return true; }
      }
      public override void InitDictionary(string rootFolder, StockDictionary dictionary, bool download)
      {
         stockDictionary = dictionary; // Save dictionary for futur use in daily download


         // Create data folder if not existing
         if (!Directory.Exists(rootFolder + ABC_DAILY_FOLDER))
         {
            Directory.CreateDirectory(rootFolder + ABC_DAILY_FOLDER);
         }
         if (!Directory.Exists(rootFolder + ABC_ARCHIVE_FOLDER))
         {
            Directory.CreateDirectory(rootFolder + ABC_ARCHIVE_FOLDER);
         }
         if (!Directory.Exists(rootFolder + ABC_INTRADAY_FOLDER))
         {
            Directory.CreateDirectory(rootFolder + ABC_INTRADAY_FOLDER);
         }
         else
         {
            foreach (string file in Directory.GetFiles(rootFolder + ABC_INTRADAY_FOLDER))
            {
               if (file != rootFolder + ABC_INTRADAY_FOLDER + "\\" + DateTime.Today.ToString("yyMMdd_") + "SBF120.csv" &&
                   file != rootFolder + ABC_INTRADAY_FOLDER + "\\" + DateTime.Today.ToString("yyMMdd_") + "IndicesFr.csv")
               {
                  File.Delete(file);
               }
            }
         }

         // Parse SBF120.txt file
         string fileName = rootFolder + CONFIG_FILE;
         InitFromFile(rootFolder, download, fileName);
         fileName = rootFolder + CONFIG_FILE_USER;
         InitFromFile(rootFolder, download, fileName);
      }

      private void InitFromFile(string rootFolder, bool download, string fileName)
      {
         if (File.Exists(fileName))
         {
            using (StreamReader sr = new StreamReader(fileName, true))
            {
               string line;
               sr.ReadLine(); // Skip first line
               while (!sr.EndOfStream)
               {
                  line = sr.ReadLine();
                  if (!line.StartsWith("#"))
                  {
                     string[] row = line.Split(';');
                     StockSerie stockSerie = new StockSerie(row[1], row[3], row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[4]), StockDataProvider.ABC);
                     if (!stockDictionary.ContainsKey(row[1]))
                     {
                        stockDictionary.Add(row[1], stockSerie);
                        if (stockSerie.StockGroup == StockSerie.Groups.CAC40)
                        {
                           StockSerie stockSerieRS = new StockSerie(row[1] + "_RS", row[3] + "_RS", StockSerie.Groups.CAC40_RS, StockDataProvider.ABC);
                           stockDictionary.Add(stockSerieRS.StockName, stockSerieRS);
                        }
                     }
                     else
                     {
                        StockLog.Write("ABC Entry: " + row[1] + " already in stockDictionary");
                     }
                     if (download && this.needDownload)
                     {
                        this.DownloadDailyData(rootFolder, stockSerie);
                     }
                  }
               }
            }
         }
      }
      public override bool LoadData(string rootFolder, StockSerie stockSerie)
      {
         bool res = false;

         if (stockSerie.StockGroup == StockSerie.Groups.CAC40_RS)
         {
            StockSerie baseSerie = stockDictionary[stockSerie.StockName.Replace("_RS", "")];
            StockSerie cacSerie = stockDictionary["CAC40"];

            return stockSerie.GenerateRelativeStrenthStockSerie(baseSerie, cacSerie);
         }

         // Read archive first
         string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_*.csv";
         string[] files = System.IO.Directory.GetFiles(rootFolder + ABC_ARCHIVE_FOLDER, fileName);
         foreach (string archiveFileName in files)
         {
            res |= ParseCSVFile(stockSerie, archiveFileName);
         }

         // Read daily value
         fileName = rootFolder + ABC_DAILY_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
         res |= ParseCSVFile(stockSerie, fileName);

         // Read intraday
         if (res && stockSerie.Keys.Last() != DateTime.Today)
         {
            if (stockSerie.BelongsToGroup(StockSerie.Groups.CAC_ALL))
            {
               fileName = rootFolder + ABC_INTRADAY_FOLDER + "\\" + DateTime.Today.ToString("yyMMdd_") + "SBF120.csv";
            }
            else
            {
               fileName = rootFolder + ABC_INTRADAY_FOLDER + "\\" + DateTime.Today.ToString("yyMMdd_") + "IndicesFr.csv";
            }
            ParseABCIntradayFile(stockSerie, fileName);
         }
         return res;
      }

      private bool ParseABCIntradayFile(StockSerie stockSerie, string fileName)
      {
         if (!File.Exists(fileName)) return false;
         using (StreamReader sr = new StreamReader(fileName, true))
         {
            string line = sr.ReadLine();
            while (!sr.EndOfStream)
            {
               string[] row = line.Split(';');
               if (row[0] == stockSerie.ISIN)
               {
                  if (DateTime.Parse(row[1]) == stockSerie.Keys.Last())
                  {
                     return false;
                  }
                  StockDailyValue dailyValue = new StockDailyValue(
                      stockSerie.StockName,
                      float.Parse(row[2], frenchCulture),
                      float.Parse(row[3], frenchCulture),
                      float.Parse(row[4], frenchCulture),
                      float.Parse(row[5], frenchCulture),
                      long.Parse(row[6], frenchCulture),
                      File.GetLastWriteTime(fileName));
                  stockSerie.Add(dailyValue.DATE, dailyValue);

                  stockSerie.ClearBarDurationCache();
                  return true;
               }
               line = sr.ReadLine();
            }
         }
         return false;
      }
      public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
      {
         if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
         {
            bool isUpTodate = false;
            stockSerie.Initialise();
            if (stockSerie.Count > 0)
            {
               DateTime lastDate = stockSerie.Keys.Last();
               bool needReloadIntraday = false;
               if (lastDate.TimeOfDay != TimeSpan.Zero)
               {
                  stockSerie.Remove(lastDate);
                  lastDate = stockSerie.Keys.Last();
                  needReloadIntraday = true;
               }

               isUpTodate = (lastDate >= DateTime.Today) ||
                   (lastDate.DayOfWeek == DayOfWeek.Friday && (DateTime.Now - lastDate).Days <= 3 && (DateTime.Today.DayOfWeek == DayOfWeek.Monday && DateTime.Now.Hour < 18)) ||
                   (lastDate == DateTime.Today.AddDays(-1) && DateTime.UtcNow.Hour < 17);

               if (!isUpTodate)
               {
                  NotifyProgress("Downloading " + stockSerie.StockGroup.ToString() + " - " + stockSerie.StockName);
                  if (lastDate.Year != DateTime.Today.Year)
                  {
                     // Happy new year !!! it's time to archive old data...
                     if (!File.Exists(rootFolder + ABC_ARCHIVE_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" + lastDate.Year.ToString() + ".csv"))
                     {
                        this.DownloadDailyFileFromABC(rootFolder + ABC_ARCHIVE_FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" + lastDate.Year.ToString() + ".csv", new DateTime(lastDate.Year, 1, 1), new DateTime(lastDate.Year, 12, 31), stockSerie.ISIN);
                     }
                  }
                  DateTime startDate = new DateTime(DateTime.Today.Year, 01, 01);
                  string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
                  this.DownloadDailyFileFromABC(rootFolder + ABC_DAILY_FOLDER, fileName, startDate, DateTime.Today, stockSerie.ISIN);

                  if (stockSerie.StockName == "CAC40") // Check if something new has been downloaded using CAC40 as the reference for all downloads
                  {
                     this.ParseCSVFile(stockSerie, rootFolder + ABC_DAILY_FOLDER + "\\" + fileName);
                     if (lastDate == stockSerie.Keys.Last())
                     {
                        this.needDownload = false;
                     }
                  }
               }
               else
               {
                  if (stockSerie.StockName == "CAC40") // Check if something new has been downloaded using CAC40 as the reference for all downloads
                  {
                     this.needDownload = false;
                  }
               }
               stockSerie.IsInitialised = isUpTodate && !needReloadIntraday;
            }
            else
            {
               NotifyProgress("Creating archive for " + stockSerie.StockName + " - " + stockSerie.StockGroup.ToString());
               DateTime lastDate = new DateTime(DateTime.Today.Year, 01, 01);
               for (int i = lastDate.Year - 1; i > 1990; i--)
               {
                  if (!this.DownloadDailyFileFromABC(rootFolder + ABC_ARCHIVE_FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" + i.ToString() + ".csv", new DateTime(i, 1, 1), new DateTime(i, 12, 31), stockSerie.ISIN))
                  {
                     break;
                  }
               }
               this.DownloadDailyFileFromABC(rootFolder + ABC_DAILY_FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv", lastDate, DateTime.Today, stockSerie.ISIN);
            }
         }
         return true;
      }
      public override bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
      {
         if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
         {
            NotifyProgress("Downloading intraday for" + stockSerie.StockGroup.ToString());

            if (!stockSerie.Initialise())
            {
               return false;
            }

            if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday
                    || DateTime.Today.DayOfWeek == DayOfWeek.Sunday
                    || stockSerie.Keys.Last() == DateTime.Today)
            {
               return false;
            }

            string folder = rootFolder + ABC_INTRADAY_FOLDER;
            string fileName;
            string item;
            if (stockSerie.BelongsToGroup(StockSerie.Groups.CAC_ALL))
            {
               fileName = DateTime.Today.ToString("yyMMdd_") + "SBF120.csv";
               item = "complet";
            }
            else
            {
               fileName = DateTime.Today.ToString("yyMMdd_") + "IndicesFr.csv";
               item = "indicesfrp";
            }
            if (File.Exists(folder + "\\" + fileName))
            {
               if (File.GetLastWriteTime(folder + "\\" + fileName) > DateTime.Now.AddMinutes(-4))
                  return false;
            }
            // TODO Check the time of the day to avoid useless download
            if (this.DownloadIntradayFileFromABC(folder, fileName, item))
            {
               // Deinitialise all the SBF120 stock
               foreach (StockSerie serie in stockDictionary.Values.Where(s => s.DataProvider == StockDataProvider.ABC))
               {
                  serie.IsInitialised = false;
               }
               foreach (StockSerie serie in stockDictionary.Values.Where(s => s.DataProvider == StockDataProvider.Breadth && s.StockName.Contains("SBF120") || s.StockName.Contains("CAC40")))
               {
                  serie.IsInitialised = false;
               }
            }
         }
         return true;
      }

      // private functions
      protected override bool ParseCSVFile(StockSerie stockSerie, string fileName)
      {
         if (File.Exists(fileName))
         {
            using (StreamReader sr = new StreamReader(fileName))
            {
               StockDailyValue readValue = null;
               while (!sr.EndOfStream)
               {
                  readValue = this.ReadMarketDataFromABCCSVStream(sr, stockSerie.StockName, true);
                  if (readValue != null)
                  {

                     if (!stockSerie.ContainsKey(readValue.DATE))
                     {
                        stockSerie.Add(readValue.DATE, readValue);
                        readValue.Serie = stockSerie;
                     }
                     else
                     { // 
                        StockLog.Write("The dailyValue already exist in the serie");
                     }
                  }
               }
            }
            return true;
         }
         else
         {
            return false;
         }
      }
      protected StockDailyValue ReadMarketDataFromABCCSVStream(StreamReader sr, string stockName, bool useAdjusted)
      {
         StockDailyValue stockValue = null;
         try
         {
            // File format
            // ISIN,Date,Open,High,Low,Close,Volume
            // FR0000120404;02/01/12;19.735;20.03;19.45;19.94;418165
            string line = sr.ReadLine().Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("<") || line.StartsWith("/")) return null;
            string[] row = line.Split(';');
            stockValue = new StockDailyValue(
                stockName,
            float.Parse(row[2], usCulture),
            float.Parse(row[3], usCulture),
            float.Parse(row[4], usCulture),
            float.Parse(row[5], usCulture),
            long.Parse(row[6], usCulture),
            DateTime.Parse(row[1], frenchCulture));
         }
         catch (System.Exception ex)
         {
            StockLog.Write(ex.Message);
         }
         return stockValue;
      }

      private bool DownloadIntradayFileFromABC(string destFolder, string fileName, string item)
      {
         bool success = true;
         try
         {
            // Build post data
            ASCIIEncoding encoding = new ASCIIEncoding();


            // Send POST request
            string url = "http://www.abcbourse.com/download/telechargement_intraday.aspx";

            if (intradayViewState == string.Empty)
            {
               // Get ViewState 
               using (WebClient webClient = new WebClient())
               {
                  webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36");
                  byte[] response = webClient.DownloadData(url);

                  string htmlContent = Encoding.ASCII.GetString(response);
                  intradayViewState = ExtractValue(htmlContent, "__VIEWSTATE");
                  intradayEventValidation = ExtractValue(htmlContent, "__EVENTVALIDATION");
               }
            }

            string postData = "__VIEWSTATE=" + intradayViewState + "&"
                + "__EVENTVALIDATION=" + intradayEventValidation + "&"
                + "ctl00%24txtAutoComplete=&"
                + "ctl00%24BodyABC%24f=ex&"
                + "ctl00%24BodyABC%24m=$ITEM&"
                + "ctl00%24BodyABC%24Button1=T%C3%A9l%C3%A9charger";

            postData = postData.Replace("$ITEM", item);

            byte[] data = Encoding.ASCII.GetBytes(postData);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.CookieContainer = new CookieContainer();
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.Method = "POST";
            req.AllowAutoRedirect = false;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add("Accept-Language", "fr,fr-fr;q=0.8,en-us;q=0.5,en;q=0.3");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36";
            req.Referer = url;

            Stream newStream = req.GetRequestStream();

            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            success = SaveResponseToFile(destFolder + @"\" + fileName, req);
         }
         catch (System.Exception ex)
         {
            StockLog.Write(ex);
            System.Windows.Forms.MessageBox.Show(ex.Message, "Connection failed");
            success = false;
         }
         return success;
      }

      private string ExtractValue(string s, string nameDelimiter)
      {
         string valueDelimiter = "value=\"";

         int viewStateNamePosition = s.IndexOf(nameDelimiter);
         int viewStateValuePosition = s.IndexOf(
               valueDelimiter, viewStateNamePosition
            );

         int viewStateStartPosition = viewStateValuePosition +
                                      valueDelimiter.Length;
         int viewStateEndPosition = s.IndexOf("\"", viewStateStartPosition);

         return HttpUtility.UrlEncode(s.Substring(viewStateStartPosition, viewStateEndPosition - viewStateStartPosition));
      }

      private bool DownloadDailyFileFromABC(string destFolder, string fileName, DateTime startDate, DateTime endDate, string ISIN)
      {
         bool success = true;
         try
         {
            // Send POST request
            string url = "http://www.abcbourse.com/download/historiques.aspx";
            if (dailyViewState == string.Empty)
            {
               // Get ViewState 
               using (WebClient webClient = new WebClient())
               {
                  webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36");
                  byte[] response = webClient.DownloadData(url);

                  string htmlContent = Encoding.ASCII.GetString(response);
                  dailyViewState = ExtractValue(htmlContent, "__VIEWSTATE");
                  dailyEventValidation = ExtractValue(htmlContent, "__EVENTVALIDATION");
               }
            }

            string postData = "ctl00_BodyABC_ToolkitScriptManager1_HiddenField=%3B%3BAjaxControlToolkit%2C+Version%3D3.0.20229.20843%2C+Culture%3Dneutral%2C+PublicKeyToken%3D28f01b0e84b6d53e%3Afr-FR%3A3b7d1b28-161f-426a-ab77-b345f2c428f5%3A865923e8%3A9b7907bc%3A411fea1c%3Ae7c87f07%3A91bd373d%3Abbfda34c%3A30a78ec5%3A9349f837%3Ad4245214%3A77c58d20%3A14b56adc%3A8e72a662%3Aacd642d2%3A596d588c%3A269a19ae&"
                + "__EVENTTARGET=&"
                + "__EVENTARGUMENT=&"
                + "__VIEWSTATE=" + dailyViewState + "&"
                + "__EVENTVALIDATION=" + dailyEventValidation + "&"
                + "ctl00%24txtAutoComplete=&"
                + "ctl00%24BodyABC%24strDateDeb=$START_DAY%2F$START_MONTH%2F$START_YEAR&"
                + "ctl00%24BodyABC%24strDateFin=$END_DAY%2F$END_MONTH%2F$END_YEAR&"
                + "ctl00%24BodyABC%24oneSico=on&"
                + "ctl00%24BodyABC%24txtOneSico=$ISIN&"
                + "ctl00%24BodyABC%24Button1=T%C3%A9l%C3%A9charger&"
                + "ctl00%24BodyABC%24dlFormat=w&"
                + "ctl00%24BodyABC%24listFormat=isin";

            postData = postData.Replace("$ISIN", ISIN);

            postData = postData.Replace("$START_DAY", startDate.Day.ToString());
            postData = postData.Replace("$START_MONTH", startDate.Month.ToString());
            postData = postData.Replace("$START_YEAR", startDate.Year.ToString());
            postData = postData.Replace("$END_DAY", endDate.Day.ToString());
            postData = postData.Replace("$END_MONTH", endDate.Month.ToString());
            postData = postData.Replace("$END_YEAR", endDate.Year.ToString());

            byte[] data = Encoding.ASCII.GetBytes(postData);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.CookieContainer = new CookieContainer();
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.Method = "POST";
            req.AllowAutoRedirect = false;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add("Accept-Language", "fr,fr-fr;q=0.8,en-us;q=0.5,en;q=0.3");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36";
            req.Referer = url;

            Stream newStream = req.GetRequestStream();
            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            success = SaveResponseToFile(destFolder + @"\" + fileName, req);
         }
         catch (System.Exception ex)
         {
            StockLog.Write(ex);
            System.Windows.Forms.MessageBox.Show(ex.Message, "Connection failed");
            success = false;
         }
         return success;
      }
      private bool SaveResponseToFile(string fileName, HttpWebRequest req)
      {
         bool success = true;
         using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
         {
            // Get the stream containing content returned by the server.
            using (Stream dataStream = response.GetResponseStream())
            {
               // Open the stream using a StreamReader for easy access.
               using (StreamReader reader = new StreamReader(dataStream))
               {
                  // Read the content.
                  string responseFromServer = reader.ReadToEnd();

                  if (responseFromServer.Length != 0 && !responseFromServer.StartsWith("<!DOCTYPE", StringComparison.CurrentCultureIgnoreCase))
                  {
                     // Save content to file
                     using (StreamWriter writer = new StreamWriter(fileName))
                     {
                        writer.Write(responseFromServer);
                        StockLog.Write("Download succeeded: " + fileName);
                     }
                  }
                  else
                  {
                     success = false;
                  }
               }
            }
         }
         return success;
      }

      public List<StockSerie> DownloadLabels()
      {
         List<StockSerie> series = new List<StockSerie>();
         try
         {
            // Build post data
            ASCIIEncoding encoding = new ASCIIEncoding();

            // Send POST request
            string url = "http://www.abcbourse.com/download/libelles.aspx";

            if (labelViewState == string.Empty)
            {
               // Get ViewState 
               using (WebClient webClient = new WebClient())
               {
                  webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36");
                  byte[] response = webClient.DownloadData(url);

                  string htmlContent = Encoding.ASCII.GetString(response);
                  labelViewState = ExtractValue(htmlContent, "__VIEWSTATE");
                  labelEventValidation = ExtractValue(htmlContent, "__EVENTVALIDATION");
               }
            }

            string postData = "__VIEWSTATE=" + labelViewState + "&"
                + "__EVENTVALIDATION=" + labelEventValidation + "&"
                + "ctl00%24txtAutoComplete=&"
                + "ctl00%24BodyABC%24xcacatp=on&"
                + "ctl00%24BodyABC%24bsp=on&"
                + "ctl00%24BodyABC%24eurolistAp=on&"
                + "ctl00%24BodyABC%24eurolistBp=on&"
                + "ctl00%24BodyABC%24eurolistCp=on&"
                + "ctl00%24BodyABC%24eurolistzep=on&"
                + "ctl00%24BodyABC%24eurolisthzep=on&"
                + "ctl00%24BodyABC%24mlp=on&"
                + "ctl00%24BodyABC%24Button1=T%C3%A9l%C3%A9charger";

            byte[] data = Encoding.ASCII.GetBytes(postData);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.CookieContainer = new CookieContainer();
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            req.Method = "POST";
            req.AllowAutoRedirect = false;
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            req.Headers.Add("Accept-Language", "fr,fr-fr;q=0.8,en-us;q=0.5,en;q=0.3");
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36";
            req.Referer = url;

            Stream newStream = req.GetRequestStream();

            // Send the data.
            newStream.Write(data, 0, data.Length);
            newStream.Close();
            bool success = false;
            int tries = 3;
            while (!success && tries > 0)
            {
               tries--;
               using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
               {
                  // Get the stream containing content returned by the server.
                  using (Stream dataStream = response.GetResponseStream())
                  {
                     using (StreamReader reader = new StreamReader(dataStream))
                     {
                        string line = reader.ReadLine();
                        if (line.StartsWith("ISIN"))
                        {
                           while (!reader.EndOfStream)
                           {
                              string[] fields = reader.ReadLine().Split(';');
                              if (fields.Length == 3)
                              {
                                 series.Add(new StockSerie(fields[1].ToUpper(), fields[2].ToUpper(), fields[0].ToUpper(), StockSerie.Groups.NONE, StockDataProvider.ABC));
                              }
                              success = true;
                           }
                        }
                     }
                  }
               }
            }
         }
         catch (System.Exception ex)
         {
            StockLog.Write(ex);
            System.Windows.Forms.MessageBox.Show(ex.Message, "Connection failed");
         }
         return series;
      }

      #region IConfigDialog Implementation
      public DialogResult ShowDialog(StockDictionary stockDico)
      {
         EuronextDataProviderConfigDlg configDlg = new EuronextDataProviderConfigDlg(stockDico);
         return configDlg.ShowDialog();
      }

      public string DisplayName
      {
         get { return "Euronext"; }
      }
      #endregion
   }
}

