using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class ABCDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private string ABC_INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\ABC";
        static private string ABC_DAILY_FOLDER = DAILY_SUBFOLDER + @"\ABC";
        static private string ABC_DAILY_CFG_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl";
        static private string ABC_DAILY_CFG_GROUP_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\group";
        static private string ABC_DAILY_CFG_SECTOR_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\sector";
        static private string FINANCIAL_SUBFOLDER = @"\data\financial";
        static private string AGENDA_SUBFOLDER = @"\data\agenda";
        static private string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\ABC";
        static private string CONFIG_FILE = @"\EuronextDownload.cfg";
        static private string CONFIG_FILE_USER = @"\EuronextDownload.user.cfg";
        static private string ABC_TMP_FOLDER = ABC_DAILY_FOLDER + @"\TMP";

        #region ABC DOWNLOAD HELPER

        private CookieContainer cookieContainer = null;
        private HttpClient client = null;
        public string verifToken = null;

        private bool Initialize()
        {
            if (this.client == null)
            {
                try
                {
                    cookieContainer = new CookieContainer();
                    this.client = new HttpClient(new HttpClientHandler { CookieContainer = cookieContainer });
                    client.BaseAddress = new Uri("https://www.abcbourse.com/");
                    var resp = client.GetAsync("download/historiques").Result;
                    if (!resp.IsSuccessStatusCode)
                        return false;

                    verifToken = FindToken("RequestVerificationToken", resp.Content.ReadAsStringAsync().Result);
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return true;
        }

        private string FindToken(string pattern, string body)
        {
            int index = body.IndexOf(pattern);
            body = body.Substring(index);
            index = body.IndexOf("value=") + 7;
            body = body.Substring(index);
            index = body.IndexOf('"');
            body = body.Remove(index);
            return body;
        }

        const string DOWNLOAD_ISIN_BODY =
            "dateFrom=$START_DATE&" +
            "dateTo=$END_DATE&" +
            "cbox=oneSico&" +
            "txtOneSico=$ISIN&" +
            "sFormat=x&" +
            "typeData=isin&" +
            "__RequestVerificationToken=$TOKEN&" +
            "cbYes=false";

        public bool DownloadISIN(string destFolder, string fileName, DateTime startDate, DateTime endDate, string ISIN)
        {
            if (!this.Initialize()) return false;

            try
            {
                var postData = DOWNLOAD_ISIN_BODY;

                postData = postData.Replace("$START_DATE", startDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$END_DATE", endDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$ISIN", ISIN);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = client.PostAsync("download/historiques", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode || resp.Content.Headers.ContentType.MediaType != "text/csv")
                    return false;
                using (var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    using (var fileStream = File.Create(Path.Combine(destFolder, fileName)))
                    {
                        respStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        const string DOWNLOAD_GROUP_BODY =
            "dateFrom=$START_DATE&" +
            "dateTo=$END_DATE&" +
            "cbox=$GROUP&" +
            "txtOneSico=&" +
            "sFormat=w&" +
            "typeData=isin&" +
            "__RequestVerificationToken=$TOKEN&"
            + "cbYes=false";
        public bool DownloadGroup(string destFolder, string fileName, DateTime startDate, DateTime endDate, string group)
        {
            if (!this.Initialize()) return false;

            try
            {
                var postData = DOWNLOAD_GROUP_BODY;

                postData = postData.Replace("$START_DATE", startDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$END_DATE", endDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = client.PostAsync("download/historiques", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode || resp.Content.Headers.ContentType.MediaType.Contains("html"))
                    return false;
                using (var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    using (var fileStream = File.Create(Path.Combine(destFolder, fileName)))
                    {
                        respStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        const string DOWNLOAD_LABEL_BODY =
            "cbox=$GROUP&" +
            "__RequestVerificationToken=$TOKEN&" +
            "cbPlace =false";
        public bool DownloadLabels(string destFolder, string fileName, string group)
        {
            if (!this.Initialize()) return false;

            try
            {
                var postData = DOWNLOAD_LABEL_BODY;

                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = client.PostAsync("download/libelles", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode)
                    return false;
                using (var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    using (var fileStream = File.Create(Path.Combine(destFolder, fileName)))
                    {
                        respStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        const string DOWNLOAD_INTRADAY_BODY =
            "sformat=ex&" +
            "splace=$GROUP&" +
            "__RequestVerificationToken=$TOKEN";

        public bool DownloadGroupIntraday(string destFolder, string fileName, string group)
        {
            if (!this.Initialize()) return false;

            try
            {
                var postData = DOWNLOAD_INTRADAY_BODY;

                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = client.PostAsync("download/telechargement_intraday", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode)
                    return false;
                using (var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    using (var fileStream = File.Create(Path.Combine(destFolder, fileName)))
                    {
                        respStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        #endregion

        public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

        private static StockDictionary stockDictionary = null;

        // IStockDataProvider Implementation
        public override bool SupportsIntradayDownload
        {
            get { return true; }
        }
        public override void InitDictionary(StockDictionary dictionary, bool download)
        {
            CreateDirectories();

            stockDictionary = dictionary; // Save dictionary for future use in daily download

            // Load Config files
            string fileName = RootFolder + CONFIG_FILE;
            InitFromFile(download, fileName);
            fileName = RootFolder + CONFIG_FILE_USER;
            InitFromFile(download, fileName);

            // Init From LBL file
            DownloadLibelleFromABC(RootFolder + ABC_DAILY_CFG_GROUP_FOLDER, "srdp", StockSerie.Groups.SRD);
            DownloadLibelleFromABC(RootFolder + ABC_DAILY_CFG_GROUP_FOLDER, "srdlop", StockSerie.Groups.SRD_LO);
            DownloadLibelleFromABC(RootFolder + ABC_DAILY_CFG_FOLDER, "eurolistAp", StockSerie.Groups.EURO_A);
            DownloadLibelleFromABC(RootFolder + ABC_DAILY_CFG_FOLDER, "eurolistBp", StockSerie.Groups.EURO_B);
            DownloadLibelleFromABC(RootFolder + ABC_DAILY_CFG_FOLDER, "eurolistCp", StockSerie.Groups.EURO_C);
            DownloadLibelleFromABC(RootFolder + ABC_DAILY_CFG_FOLDER, "eurogp", StockSerie.Groups.ALTERNEXT);
            DownloadLibelleFromABC(RootFolder + ABC_DAILY_CFG_FOLDER, "indicessecp", StockSerie.Groups.SECTORS_CAC);
            DownloadLibelleFromABC(RootFolder + ABC_DAILY_CFG_GROUP_FOLDER, "xcac40p", StockSerie.Groups.CAC40);
            DownloadLibelleFromABC(RootFolder + ABC_DAILY_CFG_FOLDER, "sp500u", StockSerie.Groups.SP500);

            // Init from Libelles
            foreach (string file in Directory.GetFiles(RootFolder + ABC_DAILY_CFG_FOLDER))
            {
                InitFromLibelleFile(file);
            }
        }

        public static void CreateDirectories()
        {
            // Create data folder if not existing
            if (!Directory.Exists(RootFolder + FINANCIAL_SUBFOLDER))
            {
                Directory.CreateDirectory(RootFolder + FINANCIAL_SUBFOLDER);
            }
            if (!Directory.Exists(RootFolder + AGENDA_SUBFOLDER))
            {
                Directory.CreateDirectory(RootFolder + AGENDA_SUBFOLDER);
            }
            if (!Directory.Exists(RootFolder + ABC_DAILY_FOLDER))
            {
                Directory.CreateDirectory(RootFolder + ABC_DAILY_FOLDER);
            }
            if (!Directory.Exists(RootFolder + ABC_TMP_FOLDER))
            {
                Directory.CreateDirectory(RootFolder + ABC_TMP_FOLDER);
            }
            if (!Directory.Exists(RootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(RootFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(RootFolder + ABC_INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(RootFolder + ABC_INTRADAY_FOLDER);
            }
            if (!Directory.Exists(RootFolder + ABC_DAILY_CFG_FOLDER))
            {
                Directory.CreateDirectory(RootFolder + ABC_DAILY_CFG_FOLDER);
            }
            if (!Directory.Exists(RootFolder + ABC_DAILY_CFG_SECTOR_FOLDER))
            {
                Directory.CreateDirectory(RootFolder + ABC_DAILY_CFG_SECTOR_FOLDER);
            }
            if (!Directory.Exists(RootFolder + ABC_DAILY_CFG_GROUP_FOLDER))
            {
                Directory.CreateDirectory(RootFolder + ABC_DAILY_CFG_GROUP_FOLDER);
            }
            else
            {
                foreach (string file in Directory.GetFiles(RootFolder + ABC_INTRADAY_FOLDER))
                {
                    // Purge intraday files at each start
                    File.Delete(file);
                }
            }
        }

        private void InitFromLibelleFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                StockSerie.Groups group = (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), Path.GetFileNameWithoutExtension(fileName));
                using (StreamReader sr = new StreamReader(fileName, true))
                {
                    string line;
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] row = line.Split(';');
                            if (!stockDictionary.ContainsKey(row[1].ToUpper()))
                            {
                                StockSerie stockSerie = new StockSerie(row[1].ToUpper(), row[2], row[0], group, StockDataProvider.ABC, BarDuration.Daily);

                                stockDictionary.Add(row[1].ToUpper(), stockSerie);
                            }
                            else
                            {
                                StockLog.Write("ABC Entry: " + row[1] + " already in stockDictionary");
                            }
                        }
                    }
                }
            }
        }

        private void InitFromFile(bool download, string fileName)
        {
            StockLog.Write("InitFromFile " + fileName);
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName, true))
                {
                    string line;
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] row = line.Split(';');
                            StockSerie stockSerie = new StockSerie(row[1], row[3], row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[4]), StockDataProvider.ABC, BarDuration.Daily);
                            if (!stockDictionary.ContainsKey(row[1]))
                            {
                                stockDictionary.Add(row[1], stockSerie);
                            }
                            else
                            {
                                StockLog.Write("ABC Entry: " + row[1] + " already in stockDictionary");
                            }
                            if (download && this.needDownload)
                            {
                                this.DownloadDailyData(stockSerie);
                            }
                        }
                    }
                }
            }
        }

        static string loadingGroup = null;
        static List<string> loadedGroups = new List<string>();
        public override bool LoadData(StockSerie stockSerie)
        {
            StockLog.Write("Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName + " - " + stockSerie.Count);

            if (stockSerie.Count == 0)
            {
                if (this.LoadFromCSV(stockSerie))
                {
                    return true;
                }
            }

            bool res = false;
            string abcGroup = GetABCGroup(stockSerie);
            string fileName = null;
            string[] files;
            if (abcGroup != null)
            {
                if (loadedGroups.Contains(abcGroup))
                {
                    return true;
                }
                try
                {
                    if (loadingGroup == null)
                    {
                        loadingGroup = abcGroup;
                    }
                    else
                    {
                        StockLog.Write("Already busy loading group: " + stockSerie.StockGroup);
                        if (loadingGroup == abcGroup)
                        {
                            do
                            {
                                Thread.Sleep(100);
                            } while (loadingGroup == abcGroup);
                            return stockSerie.Count != 0;
                        }
                        else
                        {
                            do
                            {
                                Thread.Sleep(100);
                            } while (loadingGroup == abcGroup);
                        }
                    }

                    //
                    StockLog.Write("Sync OK Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);
                    fileName = abcGroup + "_*.csv";
                    var groupFiles = Directory.GetFiles(RootFolder + ARCHIVE_FOLDER, fileName).OrderByDescending(s => s);
                    foreach (string archiveFileName in groupFiles)
                    {
                        NotifyProgress("Loading data for " + Path.GetFileNameWithoutExtension(archiveFileName));
                        if (!ParseABCGroupCSVFile(archiveFileName, stockSerie.StockGroup))
                            break;
                        else
                        {
                            res = true;
                        }
                    }
                    groupFiles = Directory.GetFiles(RootFolder + ABC_DAILY_FOLDER, fileName).OrderByDescending(s => s);
                    foreach (string currentFileName in groupFiles)
                    {
                        res = ParseABCGroupCSVFile(currentFileName, stockSerie.StockGroup);
                    }
                    fileName = Path.Combine(RootFolder + ABC_INTRADAY_FOLDER, abcGroup + ".csv");
                    if (File.Exists(fileName))
                    {
                        res = ParseABCGroupCSVFile(fileName, stockSerie.StockGroup, true);
                    }
                    loadedGroups.Add(abcGroup);

                    // Save to CSV file
                    foreach (var serie in stockDictionary.Values.Where(s => !s.StockAnalysis.Excluded && s.BelongsToGroup(stockSerie.StockGroup) && stockSerie.Count > 0))
                    {
                        this.SaveToCSV(serie);
                    }
                }
                catch (System.Exception ex)
                {
                    StockLog.Write(ex);
                }
                finally
                {
                    loadingGroup = null;
                }
            }
            else
            {
                // Read archive first
                fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_*.csv";
                files = Directory.GetFiles(RootFolder + ARCHIVE_FOLDER, fileName);
                foreach (string archiveFileName in files)
                {
                    res |= ParseCSVFile(stockSerie, archiveFileName);
                }

                // Read daily value
                fileName = RootFolder + ABC_DAILY_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
                res |= ParseCSVFile(stockSerie, fileName);
            }
            return res;
        }

        private static string GetABCGroup(StockSerie stockSerie)
        {
            string abcGroup = null;
            switch (stockSerie.StockGroup)
            {
                case StockSerie.Groups.SRD:
                    abcGroup = "srdp";
                    break;
                case StockSerie.Groups.SP500:
                    abcGroup = "sp500u";
                    break;
                case StockSerie.Groups.EURO_A:
                    abcGroup = "eurolistap";
                    break;
                case StockSerie.Groups.EURO_B:
                    abcGroup = "eurolistbp";
                    break;
                case StockSerie.Groups.EURO_C:
                    abcGroup = "eurolistcp";
                    break;
                case StockSerie.Groups.ALTERNEXT:
                    abcGroup = "eurogp";
                    break;
                case StockSerie.Groups.SECTORS_CAC:
                    abcGroup = "indicessecp";
                    break;
            }
            return abcGroup;
        }

        private bool ParseABCGroupCSVFile(string fileName, StockSerie.Groups group, bool intraday = false)
        {
            StockLog.Write(fileName);

            if (!File.Exists(fileName)) return false;
            StockSerie stockSerie = null;
            using (StreamReader sr = new StreamReader(fileName, true))
            {
                string previousISIN = string.Empty;
                DateTime date = File.GetLastWriteTime(fileName); ;
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Replace(",", ".");
                    string[] row = line.Split(';');
                    if (previousISIN != row[0])
                    {
                        stockSerie = stockDictionary.Values.FirstOrDefault(s => s.ISIN == row[0] && s.StockGroup == group);
                        previousISIN = row[0];
                    }
                    if (stockSerie != null)
                    {
                        if (intraday)
                        {
                            if (DateTime.Parse(row[1]) != DateTime.Today)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            date = DateTime.Parse(row[1]);
                        }
                        if (date.Year >= LOAD_START_YEAR)
                        {
                            if (!stockSerie.ContainsKey(date))
                            {
                                StockDailyValue dailyValue = new StockDailyValue(
                                  float.Parse(row[2]),
                                  float.Parse(row[3]),
                                  float.Parse(row[4]),
                                  float.Parse(row[5]),
                                  long.Parse(row[6]),
                                  date);
                                dailyValue.IsComplete = !intraday;
                                stockSerie.Add(dailyValue.DATE, dailyValue);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public override bool ForceDownloadData(StockSerie stockSerie)
        {
            string filePattern = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_*.csv";
            string fileName;
            StockLog.Write(stockSerie.StockName + " " + stockSerie.ISIN);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                StockLog.Write("Network is Available");
                stockSerie.ResetAllCache();
                int nbFile = 0;
                for (int i = DateTime.Today.Year - 1; i > ARCHIVE_START_YEAR; i--)
                {
                    fileName = filePattern.Replace("*", i.ToString());
                    if (!this.DownloadISIN(RootFolder + ABC_TMP_FOLDER, fileName, new DateTime(i, 1, 1), new DateTime(i, 12, 31), stockSerie.ISIN))
                    {
                        break;
                    }
                    nbFile++;
                }
                int year = DateTime.Today.Year;
                fileName = filePattern.Replace("*", year.ToString());
                if (this.DownloadISIN(RootFolder + ABC_TMP_FOLDER, fileName, new DateTime(year, 1, 1), DateTime.Today, stockSerie.ISIN))
                {
                    nbFile++;
                }
                if (nbFile == 0)
                    return false;

                // Parse loaded files
                foreach (var csvFileName in Directory.GetFiles(RootFolder + ABC_TMP_FOLDER, filePattern).OrderBy(f => f))
                {
                    using (StreamReader sr = new StreamReader(csvFileName))
                    {
                        StockDailyValue readValue = null;
                        while (!sr.EndOfStream)
                        {
                            readValue = this.ReadMarketDataFromABCCSVStream(sr, stockSerie.StockName, true);
                            if (readValue != null)
                            {
                                stockSerie.Add(readValue.DATE, readValue);
                            }
                        }
                    }
                    File.Delete(csvFileName);
                }

                this.SaveToCSV(stockSerie, true);
                stockSerie.IsInitialised = false; // @@@@

                return true;
            }
            else
            {
                StockLog.Write("Network is not Available");
                return false;
            }
        }

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            string fileName;
            StockLog.Write("DownloadDailyData Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                stockSerie.Initialise();
                if (stockSerie.Count > 0)
                {
                    DateTime lastDate = stockSerie.Keys.Last();
                    if (lastDate.TimeOfDay != TimeSpan.Zero) // Intraday data
                    {
                        stockSerie.Remove(lastDate);
                        lastDate = stockSerie.Keys.Last();
                    }
                    DateTime today = DateTime.Today;
                    bool isUpTodate = (lastDate >= today) ||
                       (today.DayOfWeek == DayOfWeek.Saturday && lastDate >= today.AddDays(-1)) ||
                       (today.DayOfWeek == DayOfWeek.Sunday && lastDate >= today.AddDays(-2)) ||
                       (today.DayOfWeek == DayOfWeek.Monday && DateTime.Now.Hour < 18 && lastDate >= today.AddDays(-2));

                    if (!isUpTodate)
                    {
                        NotifyProgress("Downloading " + stockSerie.StockGroup.ToString() + " - " + stockSerie.StockName);

                        // Happy new year !!! it's time to archive old data...
                        for (int year = lastDate.Year; year < DateTime.Today.Year; year++)
                        {
                            fileName = $"{stockSerie.ShortName}_{stockSerie.StockName}_{stockSerie.StockGroup}_{year}.csv";
                            if (!File.Exists(RootFolder + ARCHIVE_FOLDER + "\\" + fileName))
                            {
                                this.DownloadISIN(RootFolder + ARCHIVE_FOLDER, fileName, new DateTime(year, 1, 1), new DateTime(year, 12, 31), stockSerie.ISIN);
                                if (stockSerie.StockName == "CAC40")
                                {
                                    this.ParseCSVFile(stockSerie, RootFolder + ARCHIVE_FOLDER + "\\" + fileName);

                                    //this.SaveToCSV(stockSerie);
                                    //var newStockSerie = new StockSerie { ISIN = stockSerie.ISIN };
                                    //this.LoadFromCSV(newStockSerie);
                                }
                            }
                        }
                        DateTime startDate = new DateTime(DateTime.Today.Year, 01, 01);
                        fileName = $"{stockSerie.ShortName}_{stockSerie.StockName}_{stockSerie.StockGroup}.csv";
                        this.DownloadISIN(RootFolder + ABC_DAILY_FOLDER, fileName, startDate, DateTime.Today, stockSerie.ISIN);

                        if (stockSerie.StockName == "CAC40")
                        // Check if something new has been downloaded using CAC40 as the reference for all downloads
                        {
                            this.ParseCSVFile(stockSerie, RootFolder + ABC_DAILY_FOLDER + "\\" + fileName);
                            if (lastDate == stockSerie.Keys.Last())
                            {
                                this.needDownload = false;
                            }
                            else
                            {
                                DownloadMonthlyFileFromABC(RootFolder + ABC_DAILY_FOLDER, DateTime.Today, "eurolistap");
                                DownloadMonthlyFileFromABC(RootFolder + ABC_DAILY_FOLDER, DateTime.Today, "eurolistbp");
                                DownloadMonthlyFileFromABC(RootFolder + ABC_DAILY_FOLDER, DateTime.Today, "eurolistcp");
                                DownloadMonthlyFileFromABC(RootFolder + ABC_DAILY_FOLDER, DateTime.Today, "eurogp");
                                DownloadMonthlyFileFromABC(RootFolder + ABC_DAILY_FOLDER, DateTime.Today, "sp500u");
                                DownloadMonthlyFileFromABC(RootFolder + ABC_DAILY_FOLDER, DateTime.Today, "indicessecp");
                            }
                        }
                    }
                    else
                    {
                        if (stockSerie.StockName == "CAC40")
                        // Check if something new has been downloaded using CAC40 as the reference for all downloads
                        {
                            this.needDownload = false;
                        }
                    }
                    if (!isUpTodate)
                    {
                        stockSerie.IsInitialised = false;
                        stockSerie.ClearBarDurationCache();
                    }
                }
                else
                {
                    NotifyProgress("Creating archive for " + stockSerie.StockName + " - " + stockSerie.StockGroup.ToString());
                    DateTime lastDate = new DateTime(DateTime.Today.Year, 01, 01);
                    if (stockSerie.StockName == "CAC40")
                    {
                        for (int m = DateTime.Today.Month - 1; m >= 1; m--)
                        {
                            DateTime month = new DateTime(lastDate.Year, m, 1);
                            DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "eurolistap");
                            DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "eurolistbp");
                            DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "eurolistcp");
                            DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "eurogp");
                            DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "sp500u");
                            DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "indicessecp");
                        }
                    }
                    for (int i = lastDate.Year - 1; i > ARCHIVE_START_YEAR; i--)
                    {
                        if (!this.DownloadISIN(RootFolder + ARCHIVE_FOLDER, stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" +
                              i.ToString() + ".csv", new DateTime(i, 1, 1), new DateTime(i, 12, 31), stockSerie.ISIN))
                        {
                            break;
                        }
                        if (stockSerie.StockName == "CAC40")
                        {
                            for (int m = 12; m >= 1; m--)
                            {
                                DateTime month = new DateTime(i, m, 1);
                                //DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "eurolistap");
                                //DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "eurolistbp");
                                //DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "eurolistcp");
                                //DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "eurogp");
                                //DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "sp500u");
                                //DownloadMonthlyFileFromABC(RootFolder + ARCHIVE_FOLDER, month, "indicessecp");
                            }
                        }
                    }
                    this.DownloadISIN(RootFolder + ABC_DAILY_FOLDER,
                       stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv",
                       lastDate, DateTime.Today, stockSerie.ISIN);
                }
            }
            else { return false; }
            return true;
        }
        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            StockLog.Write("DownloadIntradayData Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for" + stockSerie.StockGroup.ToString());

                if (!stockSerie.Initialise() || stockSerie.Count == 0)
                {
                    return false;
                }

                if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday || stockSerie.Keys.Last() == DateTime.Today)
                {
                    return false;
                }

                string folder = RootFolder + ABC_INTRADAY_FOLDER;
                string abcGroup = GetABCGroup(stockSerie);
                if (abcGroup == null)
                    return false;
                string fileName = abcGroup + ".csv";

                if (File.Exists(folder + "\\" + fileName))
                {
                    if (File.GetLastWriteTime(folder + "\\" + fileName) > DateTime.Now.AddMinutes(-5))
                        return false;
                }

                if (this.DownloadGroupIntraday(folder, fileName, abcGroup))
                {
                    // Deinitialise all the stocks belonging to group
                    foreach (StockSerie serie in stockDictionary.Values.Where(s => s.BelongsToGroup(stockSerie.StockGroup)))
                    {
                        serie.IsInitialised = false;
                        stockSerie.ClearBarDurationCache();
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
                        if (readValue != null && readValue.DATE.Year >= LOAD_START_YEAR)
                        {
                            if (!stockSerie.ContainsKey(readValue.DATE))
                            {
                                stockSerie.Add(readValue.DATE, readValue);
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
                if (row.Length < 7)
                    return null;
                stockValue = new StockDailyValue(
                float.Parse(row[2], frenchCulture),
                float.Parse(row[3], frenchCulture),
                float.Parse(row[4], frenchCulture),
                float.Parse(row[5], frenchCulture),
                long.Parse(row[6], frenchCulture),
                DateTime.Parse(row[1], frenchCulture));
            }
            catch (Exception ex)
            {
                StockLog.Write(ex.Message);
            }
            return stockValue;
        }

        private string ExtractValue(string s, string nameDelimiter)
        {
            string valueDelimiter = "value=\"";

            int viewStateNamePosition = s.IndexOf(nameDelimiter);
            if (viewStateNamePosition == -1)
                return string.Empty;
            int viewStateValuePosition = s.IndexOf(
                  valueDelimiter, viewStateNamePosition
               );

            int viewStateStartPosition = viewStateValuePosition +
                                         valueDelimiter.Length;
            int viewStateEndPosition = s.IndexOf("\"", viewStateStartPosition);

            return HttpUtility.UrlEncode(s.Substring(viewStateStartPosition, viewStateEndPosition - viewStateStartPosition));
        }

        public static SortedDictionary<string, string> SectorCodes = new SortedDictionary<string, string>()
      {
         {"2710", "Aerospatiale et defense"},
         {"3570", "Agro-alimentaire"},
         {"3760", "Articles personnels"},
         {"8530", "Assurance - Non vie"},
         {"8570", "Assurance vie"},
         {"3350", "Automobiles et equipementiers"},
         {"8350", "Banques"},
         {"2350", "Batiment et materiaux de construction"},
         {"3530", "Boissons"},
         {"1350", "Chimie"},
         {"5370", "Distributeurs generalistes"},
         {"5330", "Distribution - Alimentation, produits pharmaceutiques"},
         {"7530", "Electricite"},
         {"3740", "Equipements de loisirs"},
         {"2730", "Equipements electroniques et electriques"},
         {"4530", "Equipements et services de sante"},
         {"7570", "Gaz, eau et services multiples aux collectivites"},
         {"8670", "Immobiliers - foncieres"},
         {"8630", "Investissements immobiliers et services"},
         {"2720", "Industries generalistes"},
         {"2750", "Ingenierie industrielle"},
         {"8980", "Instruments de placement"},
         {"8990", "Instruments de placement - hors actions"},
         {"9530", "Logiciels et services informatiques"},
         {"9570", "Materiel et equipements des technologies de l&#39;information"},
         {"5550", "Medias"},
         {"1750", "M&#233;taux industriels et mines"},
         {"0570", "Petrole - Equipements, services et distribution"},
         {"4570", "Pharmacie et biotechnologie"},
         {"0530", "Producteurs de petrole et de gaz"},
         {"3720", "Produits mnagers et bricolage"},
         {"8770", "Services financiers"},
         {"2790", "Services supports"},
         {"1730", "Sylviculture et papiers"},
         {"6530", "Telecommunications filaires"},
         {"2770", "Transport industriel"},
         {"5750", "Voyages et loisirs"}
      };

        private bool DownloadSectorFromABC(string destFolder, string sectorID)
        {
            bool success = true;
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                string fileName = destFolder + @"\" + sectorID + ".txt";
                if (File.Exists(fileName))
                {
                    if (File.GetLastWriteTime(fileName) > DateTime.Now.AddDays(-7)) // File has been updated during the last 7 days
                        return true;
                }

                try
                {
                    // Send POST request
                    string url = "http://www.abcbourse.com/download/sectors.aspx?s=%SECTORCODE%&t=3";
                    url = url.Replace("%SECTORCODE%", sectorID);

                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);


                    success = SaveResponseToFile(fileName, req);

                    if (success) MessageBox.Show("Download sector is now fixed");
                }
                catch (System.Exception ex)
                {
                    StockLog.Write(ex);
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Connection failed");
                    success = false;
                }
            }
            return success;
        }
        private bool DownloadLibelleFromABC(string destFolder, string groupName, StockSerie.Groups group)
        {
            bool success = true;
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                string fileName = destFolder + @"\" + group.ToString() + ".txt";
                if (File.Exists(fileName))
                {
                    if (File.GetLastWriteTime(fileName) > DateTime.Now.AddDays(-7)) // File has been updated during the last 7 days
                        return true;
                }

                try
                {
                    success = this.DownloadLabels(destFolder, group.ToString() + ".txt", groupName);
                }
                catch (System.Exception ex)
                {
                    StockLog.Write(ex);
                    System.Windows.Forms.MessageBox.Show(ex.Message, "Connection failed");
                    success = false;
                }
            }
            return success;
        }

        private bool DownloadMonthlyFileFromABC(string destFolder, DateTime month, string abcGroup)
        {
            bool success = true;
            try
            {
                string fileName = destFolder + @"\" + abcGroup + "_" + month.Year + "_" + month.Month + ".csv";
                if (destFolder.Contains("archive") && File.Exists(fileName))
                    return true;

                if (month.Month > 1 && DateTime.Today.Month == month.Month)
                {
                    // Force loading previous month in order to avoid missing some days
                    DownloadMonthlyFileFromABC(destFolder, new DateTime(month.Year, month.Month - 1, 1), abcGroup);
                }

                return this.DownloadGroup(destFolder, fileName, new DateTime(month.Year, month.Month, 1), new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month)), abcGroup);
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

                        if (responseFromServer.Length != 0 && !responseFromServer.StartsWith("<", StringComparison.CurrentCultureIgnoreCase))
                        {
                            // Save content to file
                            using (StreamWriter writer = new StreamWriter(fileName))
                            {
                                writer.Write(responseFromServer);
                                //StockLog.Write("Download succeeded: " + fileName);
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


        private static List<string> cac40List = null;
        public static bool BelongsToCAC40(StockSerie stockSerie)
        {
            if (cac40List == null)
            {
                cac40List = new List<string>();

                // parse CAC40 list
                string fileName = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\" +
                                  ABC_DAILY_CFG_GROUP_FOLDER + @"\CAC40.txt";
                if (File.Exists(fileName))
                {
                    using (StreamReader sr = new StreamReader(fileName, true))
                    {
                        string line;
                        sr.ReadLine(); // Skip first line
                        while (!sr.EndOfStream)
                        {
                            line = sr.ReadLine();
                            if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                            {
                                string[] row = line.Split(';');
                                cac40List.Add(row[1].ToUpper());
                            }
                        }
                    }
                }
                // Sanity Check
                //foreach (string name in cac40List)
                //{
                //   if (!stockDictionary.ContainsKey(name))
                //   {
                //      StockLog.Write("CAC 40 Stock not in dico: " + name);
                //   }
                //}
            }

            return cac40List.Contains(stockSerie.StockName);
        }
        private static List<string> srdList = null;
        public static bool BelongsToSRD(StockSerie stockSerie)
        {
            if (srdList == null)
            {
                srdList = new List<string>();

                // parse SRD list
                string fileName = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\" +
                                  ABC_DAILY_CFG_GROUP_FOLDER + @"\SRD.txt";
                if (File.Exists(fileName))
                {
                    using (StreamReader sr = new StreamReader(fileName, true))
                    {
                        string line = sr.ReadLine(); // Skip first line
                        while (!sr.EndOfStream)
                        {
                            line = sr.ReadLine();
                            if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                            {
                                string[] row = line.Split(';');
                                srdList.Add(row[1].ToUpper());
                            }
                        }
                    }
                }
                // Sanity Check
                foreach (string name in srdList)
                {
                    if (!stockDictionary.ContainsKey(name))
                    {
                        StockLog.Write("CAC40 Stock not in dico: " + name);
                    }
                }
            }

            return srdList.Contains(stockSerie.StockName);
        }
        private static List<string> srdloList = null;
        public static bool BelongsToSRD_LO(StockSerie stockSerie)
        {
            if (srdloList == null)
            {
                srdloList = new List<string>();

                // parse SRD_LO list
                string fileName = StockAnalyzerSettings.Properties.Settings.Default.RootFolder + @"\" +
                                  ABC_DAILY_CFG_GROUP_FOLDER + @"\SRD_LO.txt";
                if (File.Exists(fileName))
                {
                    using (StreamReader sr = new StreamReader(fileName, true))
                    {
                        string line;
                        sr.ReadLine(); // Skip first line
                        while (!sr.EndOfStream)
                        {
                            line = sr.ReadLine();
                            if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                            {
                                string[] row = line.Split(';');
                                srdloList.Add(row[1].ToUpper());
                            }
                        }
                    }
                }
                // Sanity Check
                foreach (string name in srdloList)
                {
                    if (!stockDictionary.ContainsKey(name))
                    {
                        StockLog.Write("CAC40 Stock not in dico: " + name);
                    }
                }
            }

            return srdloList.Contains(stockSerie.StockName);
        }

        public static void DownloadFinancialSummary(StockFinancial financial, string shortName, StockSerie stockSerie)
        {
            string url = "http://www.boursorama.com/bourse/profil/resume_societe.phtml?symbole=1r$ShortName".Replace("$ShortName", shortName);
            StockWebHelper swh = new StockWebHelper();
            string html = swh.DownloadHtml(url, null);

            WebBrowser browser = new WebBrowser();
            browser.ScriptErrorsSuppressed = true;
            browser.DocumentText = html;
            browser.Document.OpenNew(true);
            browser.Document.Write(html);
            browser.Refresh();

            HtmlDocument doc = browser.Document;

            var divs = doc.GetElementsByTagName("div").Cast<HtmlElement>();
            foreach (var div in divs)
            {
                if (div.InnerText != null && div.InnerText.StartsWith("Nombre de titres"))
                {
                    var list = div.InnerText.Replace(Environment.NewLine, "|");
                    var split = list.Split('|');
                    var nbTitres = split[0].Split(':')[1].Replace(" ", "");
                    financial.ShareNumber = long.Parse(nbTitres);
                    financial.Coupon = split.First(l => l.StartsWith("Dern")).Split(':')[1].Trim();
                    financial.Sector = split.First(l => l.StartsWith("Secteur")).Split(':')[1].Trim();
                    financial.PEA = split.First(l => l.Contains("PEA")).Split(':')[1].Trim();
                    if (stockSerie.BelongsToGroup(StockSerie.Groups.SRD))
                    {
                        financial.SRD = "Long Short";
                    }
                    if (stockSerie.BelongsToGroup(StockSerie.Groups.SRD_LO))
                    {
                        financial.SRD = "Long Only";
                    }
                    financial.Indices = split.First(l => l.StartsWith("Indice")).Split(':')[1].Trim();
                    break;
                }
            }
            foreach (var div in divs)
            {
                if (div.InnerText != null && div.InnerText.StartsWith("Prévisions des analystes"))
                {

                    var tables = div.GetElementsByTagName("table").Cast<HtmlElement>();
                    var previsions = getTableData(tables.First());

                    var dividendLine = previsions.FirstOrDefault(l => l[0] == "Dividende");
                    if (dividendLine != null)
                    {
                        float dividend = 0;
                        float.TryParse(dividendLine[1], out dividend);
                        financial.Dividend = dividend;
                    }

                    break;
                }
            }
            //foreach (var div in divs)
            //{
            //    if (div.InnerText != null && div.InnerText.StartsWith("Activité"))
            //    {
            //        Console.WriteLine(div.InnerText);
            //        financial.Activity = div.InnerHtml;
            //        break;
            //    }
            //}

        }

        public static void DownloadFinancial(StockSerie stockSerie)
        {
            if (stockSerie.Financial != null && stockSerie.Financial.DownloadDate.AddDays(7) > DateTime.Now) return;

            StockFinancial financial = new StockFinancial();
            try
            {
                string shortName = stockSerie.StockGroup == StockSerie.Groups.ALTERNEXT ? "P" : "P";
                shortName += stockSerie.ShortName;
                DownloadFinancialSummary(financial, shortName, stockSerie);

                string url = "http://www.boursorama.com/bourse/profil/profil_finance.phtml?symbole=1r$ShortName".Replace("$ShortName", shortName);
                StockWebHelper swh = new StockWebHelper();
                string html = swh.DownloadHtml(url, null);

                WebBrowser browser = new WebBrowser();
                browser.ScriptErrorsSuppressed = true;
                browser.DocumentText = html;
                browser.Document.OpenNew(true);
                browser.Document.Write(html);
                browser.Refresh();

                HtmlDocument doc = browser.Document;

                var divs = doc.GetElementsByTagName("div").Cast<HtmlElement>();
                foreach (var div in divs)
                {
                    if (div.InnerText != null && div.InnerText.StartsWith("Compte de"))
                    {
                        Console.WriteLine(div.InnerText);
                        var tables = div.GetElementsByTagName("table").Cast<HtmlElement>();
                        financial.IncomeStatement = getTableData(tables.First());
                        break;
                    }
                }
                foreach (var div in divs)
                {
                    if (div.InnerText != null && div.InnerText.StartsWith("Bilan"))
                    {
                        Console.WriteLine(div.InnerText);
                        var tables = div.GetElementsByTagName("table").Cast<HtmlElement>();
                        financial.BalanceSheet = getTableData(tables.First());
                        break;
                    }
                }
                foreach (var div in divs)
                {
                    if (div.InnerText != null && div.InnerText.StartsWith("Chiffres d'affaires"))
                    {
                        Console.WriteLine(div.InnerText);
                        var tables = div.GetElementsByTagName("table").Cast<HtmlElement>();
                        financial.Quaterly = getTableData(tables.First());
                        break;
                    }
                }

                financial.DownloadDate = DateTime.Now;
                stockSerie.Financial = financial;
                stockSerie.SaveFinancial();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void DownloadAgenda(StockSerie stockSerie)
        {
            if (!stockSerie.BelongsToGroup(StockSerie.Groups.CACALL)) return;
            if (stockSerie.Agenda == null)
            {
                stockSerie.Agenda = new StockAgenda();
            }
            else
            {
                if (stockSerie.Agenda.DownloadDate.AddMonths(1) > DateTime.Today) return;
            }

            string url = "http://www.abcbourse.com/marches/events.aspx?s=$ShortNamep".Replace("$ShortName", stockSerie.ShortName);

            StockWebHelper swh = new StockWebHelper();
            string html = swh.DownloadHtml(url, Encoding.UTF8);

            WebBrowser browser = new WebBrowser();
            browser.ScriptErrorsSuppressed = true;
            browser.DocumentText = html;
            browser.Document.OpenNew(true);
            browser.Document.Write(html);
            browser.Refresh();

            HtmlDocument doc = browser.Document;

            HtmlElementCollection tables = doc.GetElementsByTagName("table");
            List<List<string>> data = new List<List<string>>();

            foreach (HtmlElement tbl in tables)
            {
                if (tbl.InnerText.StartsWith("Date"))
                {
                    data = getTableData(tbl).Skip(1).ToList();
                    break;
                }
            }
            //
            foreach (var row in data)
            {
                if (row[0].StartsWith("du")) row[0] = row[0].Substring(row[0].IndexOf("au ") + 3);
                DateTime date = DateTime.Parse(row[0]);

                string comment = row[1];
                if (row[2] != null) comment += Environment.NewLine + row[2];

                if (!stockSerie.Agenda.ContainsKey(date))
                {
                    stockSerie.Agenda.Add(date, comment);
                }
                //else
                //{
                //    if (!stockSerie.Agenda[date].Contains(comment))
                //    {
                //        stockSerie.Agenda[date] = stockSerie.Agenda[date] + Environment.NewLine + comment;
                //    }
                //}
            }
            stockSerie.Agenda.DownloadDate = DateTime.Today;
            stockSerie.SaveAgenda();
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


        const string DATEFORMAT = "dd/MM/yyyy";
        public void SaveToCSV(StockSerie stockSerie, bool forceArchive = true)
        {
            string fileName = Path.Combine(RootFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.ShortName + ".csv");
            var values = stockSerie.Values.Where(v => v.DATE.Year < DateTime.Today.Year);
            if (values.Count() > 0)
            {
                if (forceArchive || !File.Exists(fileName))
                {
                    using (StreamWriter sw = new StreamWriter(fileName))
                    {
                        foreach (var value in values)
                        {
                            sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
                        }
                    }
                }
            }

            fileName = Path.Combine(RootFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.ShortName + ".csv");
            values = stockSerie.Values.Where(v => v.DATE.Year == DateTime.Today.Year && v.IsComplete);
            if (values.Count() > 0)
            {
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    foreach (var value in values)
                    {
                        sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
                    }
                }
            }
        }

        public bool LoadFromCSV(StockSerie stockSerie)
        {
            bool result = false;
            string fileName = Path.Combine(RootFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.ShortName + ".csv");
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] row = sr.ReadLine().Split(';');
                        var value = new StockDailyValue(float.Parse(row[1], usCulture), float.Parse(row[2], usCulture), float.Parse(row[3], usCulture), float.Parse(row[4], usCulture), long.Parse(row[5]), DateTime.ParseExact(row[0], DATEFORMAT, usCulture));
                        stockSerie.Add(value.DATE, value);
                    }
                }
                result = true;
            }
            fileName = Path.Combine(RootFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.ShortName + ".csv");
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] row = sr.ReadLine().Split(';');
                        var value = new StockDailyValue(float.Parse(row[1], usCulture), float.Parse(row[2], usCulture), float.Parse(row[3], usCulture), float.Parse(row[4], usCulture), long.Parse(row[5]), DateTime.ParseExact(row[0], DATEFORMAT, usCulture));
                        stockSerie.Add(value.DATE, value);
                    }
                }
                result = true;
            }
            return result;
        }
    }
}