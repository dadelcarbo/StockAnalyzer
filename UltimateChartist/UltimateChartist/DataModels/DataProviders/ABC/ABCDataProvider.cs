using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using UltimateChartist.Helpers;

namespace UltimateChartist.DataModels.DataProviders.ABC
{
    public class ABCDataProvider : StockDataProviderBase
    {
        public override string Name => "ABC";

        public override string DisplayName => "ABC Bourse";

        public override BarDuration[] BarDurations { get; } = { BarDuration.Daily, BarDuration.Weekly, BarDuration.Monthly };

        public override BarDuration DefaultBarDuration => BarDuration.Daily;

        #region INITIALISATION

        static string defaultConfigFile = $"ISIN;NOM;SICOVAM;TICKER;GROUP{Environment.NewLine}FR0003500008;CAC40;;CAC40;INDICES";

        public override void InitDictionary()
        {
            CreateDirectories();
            // Load Config files
            string fileName = CONFIG_FILE;
            if (!File.Exists(fileName))
            {
                File.WriteAllText(fileName, defaultConfigFile);
            }
            InitFromConfigFile(fileName);

            // Init From LBL file
            DownloadLibelleFromABC(CFG_FOLDER, StockGroup.EURO_A);
            DownloadLibelleFromABC(CFG_FOLDER, StockGroup.EURO_B);
            DownloadLibelleFromABC(CFG_FOLDER, StockGroup.EURO_C);
            DownloadLibelleFromABC(CFG_FOLDER, StockGroup.ALTERNEXT);
            DownloadLibelleFromABC(CFG_FOLDER, StockGroup.BELGIUM);
            DownloadLibelleFromABC(CFG_FOLDER, StockGroup.HOLLAND);
            DownloadLibelleFromABC(CFG_FOLDER, StockGroup.PORTUGAL);
            DownloadLibelleFromABC(CFG_FOLDER, StockGroup.SECTORS_CAC);
            DownloadLibelleFromABC(CFG_GROUP_FOLDER, StockGroup.CAC40, false, false);
            DownloadLibelleFromABC(CFG_GROUP_FOLDER, StockGroup.SBF120, false, false);
            //DownloadLibelleFromABC(Folders.DataFolder, ABC_DAILY_CFG_FOLDER, StockGroup.USA);
        }

        private string CFG_FOLDER => Path.Combine(CACHE_FOLDER, "Cfg");
        private string CFG_GROUP_FOLDER => Path.Combine(CACHE_FOLDER, @"Cfg\group");
        private string CFG_SECTOR_FOLDER => Path.Combine(CACHE_FOLDER, @"Cfg\sector");

        public void CreateDirectories()
        {
            InitCacheFolders();

            foreach (var barDuration in this.BarDurations)
            {
                var folder = Path.Combine(ARCHIVE_FOLDER, barDuration.ToString());
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            if (!Directory.Exists(Path.Combine(Folders.DataFolder, CFG_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, CFG_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, CFG_GROUP_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, CFG_GROUP_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(Folders.DataFolder, CFG_SECTOR_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(Folders.DataFolder, CFG_SECTOR_FOLDER));
            }
            if (!Directory.Exists(Path.Combine(TEMP_FOLDER, "Group")))
            {
                Directory.CreateDirectory(Path.Combine(TEMP_FOLDER, "Group"));
            }
            if (!Directory.Exists(Path.Combine(TEMP_FOLDER, "Stock")))
            {
                Directory.CreateDirectory(Path.Combine(TEMP_FOLDER, "Stock"));
            }
        }

        private void DownloadLibelleFromABC(string destFolder, StockGroup group, bool download = true, bool initLibelle = true)
        {
            string groupName = GetABCGroup(group);
            if (groupName == null)
                return;
            string fileName = destFolder + @"\" + group.ToString() + ".txt";
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (!File.Exists(fileName) || File.GetLastWriteTime(fileName) < DateTime.Now.AddDays(-7)) // File is older than 7 days
                {
                    try
                    {
                        DownloadLabels(destFolder, group.ToString() + ".txt", groupName);
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

            if (download && needDownload)
            {
                DownloadGroupData(group);
            }
        }

        private void InitFromLibelleFile(string fileName)
        {
            StockLog.Write(fileName);
            if (File.Exists(fileName))
            {
                var boursoramaDataProvider = GetDataProvider(StockDataProvider.Boursorama);
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

                            if (!Instruments.Any(i => i.Name == stockName))
                            {
                                var instrument = new Instrument
                                {
                                    Name = stockName,
                                    Symbol = row[2],
                                    ISIN = row[0],
                                    Group = group,
                                    Country = row[0].Substring(0, 2),
                                    DataProvider = this,
                                    RealTimeDataProvider = boursoramaDataProvider
                                };

                                Instruments.Add(instrument);
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

        DateTime lastLoadedCAC40Date = new DateTime(LOAD_START_YEAR, 1, 1);
        bool needDownload = false;
        private void InitFromConfigFile(string fileName)
        {
            var boursoramaDataProvider = GetDataProvider(StockDataProvider.Boursorama);
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
                            Country = row[0].Substring(0, 2),
                            Group = (StockGroup)Enum.Parse(typeof(StockGroup), row[4]),
                            DataProvider = this,
                            RealTimeDataProvider = boursoramaDataProvider
                        };
                        Instruments.Add(instrument);
                        if (instrument.Name == "CAC40")
                        {
                            var serie = instrument.GetStockSerie(BarDuration.Daily);
                            var bars = serie.Bars;
                            if (bars != null && bars.Count > 0)
                            {
                                lastLoadedCAC40Date = bars.Last().Date;
                                if (DownloadISIN(TEMP_FOLDER, "CAC40.csv", lastLoadedCAC40Date.AddDays(1), DateTime.Today, instrument.ISIN))
                                {
                                    var newBars = ReadABCFile(Path.Combine(TEMP_FOLDER, "CAC40.csv"));
                                    if (newBars != null && newBars.Count > 0)
                                    {
                                        needDownload = newBars.Max(b => b.Date) > lastLoadedCAC40Date;
                                        foreach (var newBar in newBars.Where(b => b.Date > lastLoadedCAC40Date))
                                        {
                                            bars.Add(newBar);
                                        }
                                        string cacFileName = GetCacheFilePath(instrument);
                                        StockBar.SaveCsv(bars, cacFileName, new DateTime(LOAD_START_YEAR, 1, 1));
                                    }
                                }
                            }
                            else
                            {
                                serie.Bars = ForceDownloadData(instrument);
                                needDownload = true;
                            }
                        }
                        else if (needDownload)
                        {
                            // this.DownloadISIN();
                        }
                    }
                }
            }
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

        #endregion

        public override List<StockBar> LoadData(Instrument instrument, BarDuration duration)
        {
            // Read Data from Cache
            string fileName = GetCacheFilePath(instrument); ;
            var archiveBars = StockBar.Load(fileName, new DateTime(LOAD_START_YEAR, 1, 1));
            var tmpFileName = Path.Combine(TEMP_FOLDER, "Stock", GetFileName(instrument));
            if (archiveBars == null)
            {
                var bars = StockBar.Load(tmpFileName);
                if (bars == null)
                {
                    return null;
                }
                else
                {
                    StockBar.SaveCsv(bars, fileName);
                    return bars;
                }
            }
            else
            {
                var bars = StockBar.Load(tmpFileName, archiveBars.Last().Date);
                if (bars == null || bars.Count == 0)
                {
                    return archiveBars;
                }
                else
                {
                    archiveBars.AddRange(bars);
                    StockBar.SaveCsv(archiveBars, fileName);
                    File.Delete(tmpFileName);
                    return archiveBars;
                }
            }
        }
        public override List<StockBar> DownloadData(Instrument instrument, BarDuration duration)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return null;

            return null;
        }
        public List<StockBar> ForceDownloadData(Instrument instrument)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return null;

            string filePattern = instrument.ISIN + "_" + instrument.Symbol + "_*.csv";
            string fileName;
            StockLog.Write(instrument.Name + " " + instrument.ISIN);
            int nbFile = 0;
            var folder = TEMP_FOLDER;
            for (int i = DateTime.Today.Year - 1; i >= LOAD_START_YEAR; i--)
            {
                fileName = filePattern.Replace("*", i.ToString());
                if (!DownloadISIN(folder, fileName, new DateTime(i, 1, 1), new DateTime(i, 12, 31), instrument.ISIN))
                {
                    break;
                }
                nbFile++;
            }
            int year = DateTime.Today.Year;
            fileName = filePattern.Replace("*", year.ToString());
            if (DownloadISIN(folder, fileName, new DateTime(year, 1, 1), DateTime.Today, instrument.ISIN))
            {
                nbFile++;
            }
            if (nbFile == 0)
                return null;

            // Parse loaded files
            var bars = new List<StockBar>();
            foreach (var csvFileName in Directory.GetFiles(folder, filePattern).OrderBy(f => f))
            {
                bars.AddRange(ReadABCFile(csvFileName));
                File.Delete(csvFileName);
            }
            StockBar.SaveCsv(bars, GetCacheFilePath(instrument));
            return bars;
        }

        private List<StockBar> ReadABCFile(string csvFileName)
        {
            using (StreamReader sr = new StreamReader(csvFileName))
            {
                var bars = new List<StockBar>();
                while (!sr.EndOfStream)
                {
                    var readValue = ReadABCLine(sr);
                    if (readValue != null)
                    {
                        bars.Add(readValue);
                    }
                }
                return bars;
            }
        }

        private bool DownloadGroupData(StockGroup group)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            if (DownloadMonthlyFileFromABC(Path.Combine(TEMP_FOLDER, "Group"), lastLoadedCAC40Date.AddDays(1), DateTime.Today, group))
            {
                LoadGroupData(group);
            }

            return true;
        }
        private bool DownloadMonthlyFileFromABC(string destFolder, DateTime startDate, DateTime endDate, StockGroup group)
        {
            bool success = false;
            NotifyProgress($"Downloading data for {group} from {startDate.ToShortDateString()}");
            try
            {
                while (endDate - startDate >= new TimeSpan(31, 0, 0, 0))
                {
                    var endOfMonth = new DateTime(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);
                    success |= DownloadMonthlyFileFromABC(destFolder, startDate, endOfMonth, group);
                    startDate = endOfMonth.AddDays(1);
                }

                string fileName = destFolder + @"\" + group + "_" + endDate.Year + "_" + endDate.Month + ".csv";

                success |= DownloadGroup(destFolder, fileName, startDate, endDate, group);
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                MessageBox.Show(ex.Message, "Connection failed");
                success = false;
            }
            return success;
        }
        private void LoadGroupData(StockGroup group)
        {
            StockLog.Write("Group: " + group);
            try
            {
                string filePattern = group + "_*.csv";

                var lines = Directory.GetFiles(Path.Combine(TEMP_FOLDER, "Group"), filePattern).OrderBy(s => s).Select(s => File.ReadAllLines(s)).ToList();
                foreach (string currentFile in Directory.GetFiles(Path.Combine(TEMP_FOLDER, "Group"), filePattern))
                {
                    File.Delete(currentFile);
                }

                var fileName = Path.Combine(TEMP_FOLDER, "Group", group + ".csv");
                if (File.Exists(fileName))
                {
                    lines.Add(File.ReadAllLines(fileName));
                    File.Delete(fileName);
                }

                // Save to CSV file
                if (lines.Count() > 0)
                {
                    NotifyProgress($"Saving files for {group}");
                    var isinBars = ParseABCGroupCSVFile(lines.SelectMany(l => l));
                    foreach (var isinBar in isinBars)
                    {
                        var instrument = Instruments.FirstOrDefault(i => i.ISIN == isinBar.Key);
                        if (instrument != null)
                        {
                            var stockSerie = instrument.GetStockSerie(BarDuration.Daily);
                            if (stockSerie.Bars == null)
                            {
                                stockSerie.Bars = isinBar.Value.ToList();
                            }
                            else
                            {
                                stockSerie.Bars.AddRange(isinBar.Value);
                            }
                            StockBar.SaveCsv(isinBar.Value, GetCacheFilePath(instrument));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
        }
        private SortedDictionary<string, IEnumerable<StockBar>> ParseABCGroupCSVFile(IEnumerable<string> lines)
        {
            try
            {
                var bars = new SortedDictionary<string, IEnumerable<StockBar>>();

                var data = lines.Select(l => l.Replace(",", ".").Split(';'));
                foreach (var l in data.GroupBy(d => d[0]))
                {
                    bars.Add(l.Key, l.Select(v => new StockBar(
                                        DateTime.Parse(v[1]),
                                      decimal.Parse(v[2]),
                                      decimal.Parse(v[3]),
                                      decimal.Parse(v[4]),
                                      decimal.Parse(v[5]),
                                      long.Parse(v[6]))));
                }
                return bars;
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            return null;
        }
        protected StockBar ReadABCLine(StreamReader sr)
        {
            StockBar stockValue = null;
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
                stockValue = new StockBar(
                DateTime.Parse(row[1], frenchCulture),
                decimal.Parse(row[2], frenchCulture),
                decimal.Parse(row[3], frenchCulture),
                decimal.Parse(row[4], frenchCulture),
                decimal.Parse(row[5], frenchCulture),
                long.Parse(row[6], frenchCulture));
            }
            catch (Exception ex)
            {
                StockLog.Write(ex.Message);
            }
            return stockValue;
        }

        #region ABC DOWNLOAD HELPER

        private CookieContainer cookieContainer = null;
        private HttpClient httpClient = null;
        public string verifToken = null;

        private bool Initialize()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;
            if (httpClient == null)
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
            if (!Initialize())
                return false;

            try
            {
                if (endDate < startDate)
                    return false;
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
        public bool DownloadGroup(string destFolder, string fileName, DateTime startDate, DateTime endDate, StockGroup group)
        {
            if (!Initialize())
                return false;

            try
            {
                var postData = DOWNLOAD_GROUP_BODY;

                postData = postData.Replace("$START_DATE", startDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$END_DATE", endDate.ToString("yyyy-MM-dd"));
                postData = postData.Replace("$GROUP", GetABCGroup(group));
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
            if (!Initialize())
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
            if (!Initialize())
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
            if (!Initialize()) return false;

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
    }
}
