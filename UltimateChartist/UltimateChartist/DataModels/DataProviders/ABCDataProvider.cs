using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using UltimateChartist.Helpers;

namespace UltimateChartist.DataModels.DataProviders
{
    public class ABCDataProvider : StockDataProviderBase
    {
        static private string ABC_INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\ABC";
        static private string ABC_DAILY_FOLDER = DAILY_SUBFOLDER + @"\ABC";
        static private string ABC_DAILY_CFG_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl";
        static private string ABC_DAILY_CFG_GROUP_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\group";
        static private string ABC_DAILY_CFG_SECTOR_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\sector";
        static private string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\ABC";
        static private string CONFIG_FILE = "EuronextDownload.cfg";
        static private string CONFIG_FILE_USER = "EuronextDownload.user.cfg";
        static private string ABC_TMP_FOLDER = ABC_DAILY_FOLDER + @"\TMP";

        #region ABC DOWNLOAD HELPER

        private CookieContainer cookieContainer = null;
        private HttpClient httpClient = null;
        public string verifToken = null;

        private bool Initialize()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return false;
            if (this.httpClient == null)
            {
                try
                {
                    cookieContainer = new CookieContainer();
                    var httpClient = new HttpClient(new HttpClientHandler { CookieContainer = cookieContainer });
                    httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };
                    httpClient.BaseAddress = new Uri("https://www.abcbourse.com/");

                    var resp = httpClient.GetAsync("download/historiques").GetAwaiter().GetResult();
                    if (!resp.IsSuccessStatusCode)
                    {
                        StockLog.Write("Failed initializing ABC Provider HttpClient: " + resp.Content.ReadAsStringAsync().Result);
                        return false;
                    }

                    verifToken = FindToken("RequestVerificationToken", resp.Content.ReadAsStringAsync().Result);
                    this.httpClient = httpClient;
                }
                catch (Exception ex)
                {
                    StockLog.Write(ex);
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

                var resp = httpClient.PostAsync("download/historiques", content).GetAwaiter().GetResult();
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
            catch (Exception)
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
            if (!this.Initialize())
                return false;

            try
            {
                var postData = DOWNLOAD_GROUP_BODY;

                postData = postData.Replace("$START_DATE", startDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$END_DATE", endDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                StockLog.Write("Downloading group: " + postData);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = httpClient.PostAsync("download/historiques", content).GetAwaiter().GetResult();
                if (!resp.IsSuccessStatusCode || resp.Content.Headers.ContentType.MediaType.Contains("html"))
                {
                    StockLog.Write("Failed downloading group: " + resp.Content.ReadAsStringAsync().Result);
                    return false;
                }
                using (var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    using (var fileStream = File.Create(Path.Combine(destFolder, fileName)))
                    {
                        respStream.CopyTo(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                return false;
            }
            return true;
        }

        const string DOWNLOAD_INTRADAY_GROUP_BODY =
            "splace=$GROUP&" +
            "sformat=w&" +
            "__RequestVerificationToken=$TOKEN";
        public bool DownloadIntradayGroup(string destFolder, string fileName, string group)
        {
            StockLog.Write(group);
            if (!this.Initialize())
                return false;

            var now = DateTime.Now.TimeOfDay;
            if (now < TimeSpan.FromHours(9) || now > TimeSpan.FromHours(17.5))
                return false;

            try
            {
                var postData = DOWNLOAD_INTRADAY_GROUP_BODY;

                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = httpClient.PostAsync("download/telechargement_intraday", content).GetAwaiter().GetResult();
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
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        const string DOWNLOAD_LABEL_BODY =
            "cbox=$GROUP&" +
            "__RequestVerificationToken=$TOKEN&" +
            "cbPlace=false";
        public bool DownloadLabels(string destFolder, string fileName, string group)
        {
            if (!this.Initialize())
                return false;

            try
            {
                var postData = DOWNLOAD_LABEL_BODY;

                postData = postData.Replace("$GROUP", group);
                postData = postData.Replace("$TOKEN", verifToken);

                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                var resp = httpClient.PostAsync("download/libelles", content).GetAwaiter().GetResult();
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
            catch (Exception)
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

                var resp = httpClient.PostAsync("download/telechargement_intraday", content).GetAwaiter().GetResult();
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
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        #endregion
        public override string DisplayName => "ABC Bourse";

        public override void InitDictionary()
        {
            CreateDirectories();

            // Init From LBL file
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.EURO_A);
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.EURO_B);
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.EURO_C);
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.ALTERNEXT);
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.BELGIUM);
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.HOLLAND);
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.PORTUGAL);
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_GROUP_FOLDER, StockGroup.CAC40, false);
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_GROUP_FOLDER, StockGroup.SBF120, false);
            DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.SECTORS_CAC);
            //DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.USA);

            // Load Config files
            string fileName = Path.Combine(Folders.PersonalFolder, CONFIG_FILE);
            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, defaultConfigFile);
            }
            InitFromFile(fileName);
            fileName = Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER);
            InitFromFile(fileName);
            foreach (var g in this.Instruments.GroupBy(s => s.Group))
            {
                StockLog.Write($"Group: {g.Key} prefix: {g.Select(s => s.ISIN.Substring(0, 2)).Distinct().Aggregate((i, j) => i + " " + j)}");
            }
        }

        public static void CreateDirectories()
        {
            if (!Directory.Exists(Folders.AgendaFolder))
            {
                Directory.CreateDirectory(Folders.AgendaFolder);
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, ABC_DAILY_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, ABC_DAILY_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder + ARCHIVE_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder + ARCHIVE_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, ABC_INTRADAY_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, ABC_INTRADAY_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, ABC_DAILY_CFG_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, ABC_DAILY_CFG_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, ABC_DAILY_CFG_SECTOR_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, ABC_DAILY_CFG_SECTOR_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, ABC_DAILY_CFG_GROUP_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, ABC_DAILY_CFG_GROUP_FOLDER));
            }
            else
            {
                foreach (string file in Directory.GetFiles(Path.Combine(Folders.DataFolder, ABC_INTRADAY_FOLDER)))
                {
                    // Purge files at each start
                    File.Delete(file);
                }
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, ABC_TMP_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, ABC_TMP_FOLDER));
            }
            else
            {
                foreach (string file in Directory.GetFiles(Path.Combine(Folders.DataFolder, ABC_TMP_FOLDER)))
                {
                    // Purge files at each start
                    File.Delete(file);
                }
            }
        }

        public string UserConfigFileName { get { return CONFIG_FILE_USER; } }

        static string defaultConfigFile = "ISIN;NOM;SICOVAM;TICKER;GROUP" + Environment.NewLine + "FR0003500008;CAC40;;CAC40;INDICES";

        private void DownloadLibelleFromABC(string rootFolder, string subFolder, StockGroup group, bool initLibelle = true)
        {
            var destFolder = Path.Combine(rootFolder, subFolder);
            string groupName = GetABCGroup(group);
            if (groupName == null)
                return;
            string fileName = destFolder + @"\" + group.ToString() + ".txt";
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                if (!File.Exists(fileName) || File.GetLastWriteTime(fileName) < DateTime.Now.AddDays(-7)) // File is older than 7 days
                {
                    try
                    {
                        this.DownloadLabels(destFolder, group.ToString() + ".txt", groupName);
                    }
                    catch (Exception ex)
                    {
                        StockLog.Write(ex);
                    }
                }
            }

            if (initLibelle)
            {
                InitFromLibelleFile(fileName);
            }
        }

        private static string GetABCGroup(StockGroup stockGroup)
        {
            string abcGroup = null;
            switch (stockGroup)
            {
                case StockGroup.EURO_A:
                    abcGroup = "eurolistap";
                    break;
                case StockGroup.EURO_B:
                    abcGroup = "eurolistbp";
                    break;
                case StockGroup.EURO_C:
                    abcGroup = "eurolistcp";
                    break;
                case StockGroup.ALTERNEXT:
                    abcGroup = "eurogp";
                    break;
                case StockGroup.SECTORS_CAC:
                    abcGroup = "indicessecp";
                    break;
                case StockGroup.BELGIUM:
                    abcGroup = "belg";
                    break;
                case StockGroup.HOLLAND:
                    abcGroup = "holln";
                    break;
                case StockGroup.PORTUGAL:
                    abcGroup = "lisboal";
                    break;
                case StockGroup.CAC40:
                    abcGroup = "xcac40p";
                    break;
                case StockGroup.SBF120:
                    abcGroup = "xsbf120p";
                    break;
                case StockGroup.USA:
                    abcGroup = "usau";
                    break;
                //case StockGroup.GERMANY:
                //    abcGroup = "germanyf";
                //    break;
                //case StockGroup.SPAIN:
                //    abcGroup = "spainm";
                //    break;
                //case StockGroup.ITALIA:
                //    abcGroup = "italiai";
                //    break;
                default:
                    StockLog.Write($"StockGroup {stockGroup} is not supported in ABC Bourse");
                    break;
            }
            return abcGroup;
        }
        private bool IsinMatchGroup(StockGroup group, string line)
        {
            switch (group)
            {
                case StockGroup.EURO_A:
                case StockGroup.EURO_B:
                case StockGroup.EURO_C:
                case StockGroup.ALTERNEXT:
                    return true;
                case StockGroup.BELGIUM:
                    return line.StartsWith("BE");
                case StockGroup.HOLLAND:
                    return line.StartsWith("NL");
                case StockGroup.PORTUGAL:
                    return line.StartsWith("PT");
                case StockGroup.SECTORS_CAC:
                    return line.StartsWith("QS");
                case StockGroup.USA:
                    return line.StartsWith("US");
                    //case StockGroup.GERMANY:
                    //    return line.StartsWith("DE");
                    //case StockGroup.ITALIA:
                    //    return line.StartsWith("IT");
                    //case StockGroup.SPAIN:
                    //    return line.StartsWith("ES");
            }
            throw new ArgumentException($"Group: {group} not supported in ABC");
        }


        private void InitFromLibelleFile(string fileName)
        {
            StockLog.Write(fileName);
            if (File.Exists(fileName))
            {
                StockGroup group = (StockGroup)Enum.Parse(typeof(StockGroup), Path.GetFileNameWithoutExtension(fileName));
                using (StreamReader sr = new StreamReader(fileName, true))
                {
                    string line;
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line) && IsinMatchGroup(group, line))
                        {
                            string[] row = line.Split(';');
                            string stockName = row[1].ToUpper().Replace(" - ", " ").Replace("-", " ").Replace("  ", " ");

                            if (!this.Instruments.Any(i => i.Name == stockName))
                            {
                                var instrument = new Instrument
                                {
                                    Name = stockName,
                                    Symbol = row[2],
                                    ISIN = row[0],
                                    Group = group,
                                    DataProvider = StockDataProvider.ABC
                                };
                                this.Instruments.Add(instrument);
                            }
                            else
                            {
                                StockLog.Write(line + " already exists");
                            }
                        }
                    }
                }
            }
            else
            {
                StockLog.Write("File does not exist");
            }
        }
        private void InitFromFile(string fileName)
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
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] row = line.Split(';');
                            Instrument instrument = new Instrument()
                            {
                                Name = row[1],
                                Symbol = row[3],
                                ISIN = row[0],
                                Group = (StockGroup)Enum.Parse(typeof(StockGroup), row[4]),
                                DataProvider = StockDataProvider.ABC
                            };
                            this.Instruments.Add(instrument);
                        }
                    }
                }
            }
        }

        public override List<StockBar> LoadData(Instrument instrument, BarDuration duration)
        {
            // Read archive first
            string fileName = instrument.ISIN + "_" + instrument.Symbol + ".csv";
            string fullFileName = Path.Combine(Folders.DataFolder, DAILY_ARCHIVE_SUBFOLDER, instrument.DataProvider.ToString(), fileName);
            var archiveBars = StockBar.Load(fullFileName, new DateTime(LOAD_START_YEAR, 1, 1));

            fullFileName = Path.Combine(Folders.DataFolder, DAILY_SUBFOLDER, instrument.DataProvider.ToString(), fileName);
            var bars = StockBar.Load(fullFileName, new DateTime(LOAD_START_YEAR, 1, 1));
            if (archiveBars != null)
            {
                if (bars != null) archiveBars.AddRange(bars);
                return archiveBars;
            }
            else { return bars; }
        }
    }
}
