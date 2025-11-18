using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static StockAnalyzer.StockClasses.StockSerie;

namespace StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider
{
    public enum Market
    {
        EURONEXT,
        XETRA,
        NYSE,
        MIXED
    }

    public class ABCDataProvider : StockDataProviderBase, IConfigDialog
    {
        private static readonly string ABC_INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\ABC";
        private static readonly string ABC_DAILY_FOLDER = DAILY_SUBFOLDER + @"\ABC";
        private static readonly string ABC_DAILY_CFG_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl";
        private static readonly string ABC_DAILY_CFG_GROUP_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\group";
        private static readonly string ABC_DAILY_CFG_SECTOR_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\sector";
        private static readonly string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\ABC";
        private static readonly string CONFIG_FILE = "EuronextDownload.cfg";
        private static readonly string ABC_TMP_FOLDER = ABC_DAILY_FOLDER + @"\TMP";
        private static readonly string ABC_WEB_CACHE_FOLDER = ABC_DAILY_FOLDER + @"\WebCache";
        private static readonly string ABC_WEB_PROCESSED_FOLDER = ABC_DAILY_FOLDER + @"\Processed";
        private static readonly string ABC_TMP_CACHE_FOLDER = ABC_DAILY_FOLDER + @"\TmpCache";

        public string UserConfigFileName => CONFIG_FILE;

        private static StockDictionary stockDictionary = null;

        public override bool SupportsIntradayDownload => Settings.Default.SupportIntraday;

        static readonly string defaultConfigFileContent = "ISIN;NOM;SICOVAM;TICKER;GROUP" + Environment.NewLine + "FR0003500008;CAC40;;CAC40;INDICES";

        static string configPath => Path.Combine(Folders.PersonalFolder, "AbcDownloadConfig.txt");

        List<AbcDownloadHistory> downloadHistory;
        static string historyFileName = Path.Combine(DataFolder + ABC_DAILY_CFG_FOLDER, "DownloadHistory.txt");

        List<AbcGroupDownloadHistory> groupDownloadHistory;
        static string groupHistoryFileName = Path.Combine(DataFolder + ABC_DAILY_CFG_FOLDER, "GroupDownloadHistory.txt");

        static string excludeFileName = Path.Combine(Folders.PersonalFolder, "AbcExclude.txt");

        static List<ABCGroup> abcGroupConfig = null;
        static List<string> excludeList = new List<string>();

        static readonly Dictionary<Market, TimeSpan> marketDownloadTimes = new Dictionary<Market, TimeSpan>
        {
            { Market.EURONEXT, new TimeSpan(18, 15, 0) },
            { Market.XETRA, new TimeSpan(18, 30, 0) },
            { Market.NYSE, new TimeSpan(6, 0, 0) }
        };

        public override void InitDictionary(StockDictionary dictionary, bool download)
        {
            stockDictionary = dictionary;
            CreateDirectories();

            if (File.Exists(excludeFileName))
            {
                excludeList = File.ReadAllLines(excludeFileName).ToList();
            }

            if (!File.Exists(configPath))
            {
                MessageBox.Show("The default ABC configuration file is missing in:" + Environment.NewLine + configPath, "ABC Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            abcGroupConfig = JsonSerializer.Deserialize<List<ABCGroup>>(File.ReadAllText(configPath), new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });

            #region Create Series - no Download (but lalels)
            // Load Config files
            string configFileName = Path.Combine(Folders.PersonalFolder, UserConfigFileName);
            if (!File.Exists(configFileName))
            {
                File.WriteAllText(configFileName, defaultConfigFileContent);
            }
            InitFromFile(configFileName);

            // Intialize Groups
            foreach (var config in abcGroupConfig)
            {
                InitAbcGroup(config, download);
            }
            #endregion

            GetDownloadHistory(historyFileName);
            groupDownloadHistory = AbcGroupDownloadHistory.Load(groupHistoryFileName);

            if (download)
            {
                AbcClient.CacheFolder = DataFolder + ABC_WEB_CACHE_FOLDER;

                // Download individual stocks
                DownloadFromConfigFile(configFileName);

                // Download groups
                foreach (var config in abcGroupConfig.Where(c => !c.LabelOnly))
                {
                    NotifyProgress($"Downloading data for {config.Group}");
                    this.DownloadGroupFromAbc(config);
                    AbcGroupDownloadHistory.Save(groupHistoryFileName, groupDownloadHistory);

                    NotifyProgress($"Processing data for {config.Group}");
                    LoadDataFromFolder(DataFolder + ABC_TMP_FOLDER);

                    AbcDownloadHistory.Save(historyFileName, downloadHistory);
                }
            }
            StockDictionary.Instance["CAC40"].Initialise();
        }

        private void GetDownloadHistory(string historyFileName)
        {
            // Individual stock history
            downloadHistory = AbcDownloadHistory.Load(historyFileName);
            if (downloadHistory.Count == 0)
            {
                foreach (var group in StockDictionary.Instance.Values.Where(s => s.DataProvider == StockDataProvider.ABC).GroupBy(s => s.StockGroup))
                {
                    NotifyProgress($"Generating download history for {group.Key}");
                    foreach (var stockSerie in group)
                    {
                        var lastDate = this.GetLastDate(stockSerie);
                        if (lastDate > DateTime.MinValue)
                        {
                            var history = new AbcDownloadHistory(stockSerie.ISIN, lastDate, stockSerie.StockName, stockSerie.StockGroup.ToString());
                            downloadHistory.Add(history);
                        }
                    }
                }

                AbcDownloadHistory.Save(historyFileName, downloadHistory);
            }
        }

        private void LoadDataFromFolder(string downloadPath)
        {
            var dataFiles = Directory.EnumerateFiles(downloadPath, "*.csv").OrderByDescending(f => File.GetCreationTime(f)).OrderBy(f => f).GroupBy(f => Path.GetFileNameWithoutExtension(f).Split('_')[0]);

            foreach (var fileGroup in dataFiles)
            {
                var lines = fileGroup.SelectMany(f => File.ReadAllLines(f));
                LoadDataFromAbcLines(lines);

                foreach (var f in fileGroup)
                {
                    var destFile = f.Replace(ABC_TMP_FOLDER, ABC_WEB_PROCESSED_FOLDER);
                    if (File.Exists(destFile))
                        File.Delete(destFile);
                    File.Move(f, destFile);
                }
            }
        }
        private void LoadDataFromAbcLines(IEnumerable<string> lines)
        {
            var splitLines = lines.Select(l => l.Split(';'));
            foreach (var serieData in splitLines.GroupBy(l => l[0]))
            {
                var abcId = serieData.Key.Length == 12 ? serieData.Key + 'p' : serieData.Key;
                var stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.AbcId == abcId);
                if (stockSerie == null)
                    continue;

                var history = downloadHistory.FirstOrDefault(h => h.Id == stockSerie.ISIN);
                if (history == null)
                {
                    this.LoadFromCSV(stockSerie, false);

                    history = new AbcDownloadHistory(stockSerie.ISIN, stockSerie.Count > 0 ? stockSerie.Values.Last().DATE : DateTime.MinValue, stockSerie.StockName, stockSerie.StockGroup.ToString());
                    downloadHistory.Add(history);
                }

                var dailyValues = serieData.Where(row => DateTime.Parse(row[1], frenchCulture) > history.LastDate).Select(row => new StockDailyValue(
                    float.Parse(row[2], CultureInfo.InvariantCulture),
                    float.Parse(row[3], CultureInfo.InvariantCulture),
                    float.Parse(row[4], CultureInfo.InvariantCulture),
                    float.Parse(row[5], CultureInfo.InvariantCulture),
                    long.Parse(row[6], frenchCulture),
                    DateTime.Parse(row[1], frenchCulture))).OrderBy(d => d.DATE).ToList();

                if (dailyValues.Count > 0)
                {
                    if (stockSerie.Count == 0)
                        this.LoadFromCSV(stockSerie, false);

                    foreach (var dailyValue in dailyValues)
                    {
                        stockSerie.Add(dailyValue.DATE, dailyValue);
                        history.LastDate = dailyValue.DATE;
                    }
                    this.SaveToCSV(stockSerie, false); // Not true need to detect if need to save archive
                }
                stockSerie.IsInitialised = false;
            }
        }


        private void LoadDataFromCotations()
        {
            string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            downloadPath = System.IO.Path.Combine(downloadPath, "Downloads");
            var dataFile = Directory.EnumerateFiles(downloadPath, "Cotations*.csv").OrderByDescending(f => File.GetCreationTime(f)).FirstOrDefault();

            if (string.IsNullOrEmpty(null) || !File.Exists(dataFile))
                return;

            LoadDataFromCotations(dataFile);
        }
        private void LoadDataFromCotations(string dataFile)
        {
            var lines = File.ReadAllLines(dataFile).Select(l => l.Split(';'));
            foreach (var serieData in lines.GroupBy(l => l[0]))
            {
                var stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == serieData.Key);
                if (stockSerie == null)
                    continue;

                var history = downloadHistory.FirstOrDefault(h => h.Id == stockSerie.ISIN);
                if (history == null)
                {
                    this.LoadFromCSV(stockSerie, false);

                    history = new AbcDownloadHistory(stockSerie.ISIN, stockSerie.Count > 0 ? stockSerie.Values.Last().DATE : DateTime.MinValue, stockSerie.StockName, stockSerie.StockGroup.ToString());
                    downloadHistory.Add(history);
                }

                var dailyValues = serieData.Where(row => DateTime.Parse(row[1], frenchCulture) > history.LastDate).Select(row => new StockDailyValue(
                    float.Parse(row[2], CultureInfo.InvariantCulture),
                    float.Parse(row[3], CultureInfo.InvariantCulture),
                    float.Parse(row[4], CultureInfo.InvariantCulture),
                    float.Parse(row[5], CultureInfo.InvariantCulture),
                    long.Parse(row[6], frenchCulture),
                    DateTime.Parse(row[1], frenchCulture))).OrderBy(d => d.DATE).ToList();

                if (dailyValues.Count > 0)
                {
                    if (stockSerie.Count == 0)
                        this.LoadFromCSV(stockSerie, false);

                    foreach (var dailyValue in dailyValues)
                    {
                        stockSerie.Add(dailyValue.DATE, dailyValue);
                        history.LastDate = dailyValue.DATE;
                    }
                    this.SaveToCSV(stockSerie, false);
                }
                stockSerie.IsInitialised = false;
            }

            File.Delete(dataFile);
        }

        private void LoadDataFromSeance()
        {
            string downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            downloadPath = System.IO.Path.Combine(downloadPath, "Downloads");
            var dataFiles = Directory.EnumerateFiles(downloadPath, "Seance*.csv").OrderByDescending(f => File.GetCreationTime(f));

            foreach (var dataFile in dataFiles)
            {
                var date = File.GetLastWriteTime(dataFile);
                if (date.Date == DateTime.Today)
                {
                    var lines = File.ReadAllLines(dataFile).Select(l => l.Split(';'));
                    foreach (var row in lines)
                    {
                        var stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == row[0]);
                        if (stockSerie == null)
                            continue;

                        if (stockSerie.Count > 0)
                            stockSerie.IsInitialised = false;

                        this.LoadFromCSV(stockSerie);
                        if (stockSerie.Count == 0)
                            continue;

                        var dailyValue = new StockDailyValue(
                        float.Parse(row[2], frenchCulture),
                        float.Parse(row[3], frenchCulture),
                        float.Parse(row[4], frenchCulture),
                        float.Parse(row[5], frenchCulture),
                        long.Parse(row[6], frenchCulture),
                        date);

                        stockSerie.Add(date, dailyValue);
                    }
                }
                File.Delete(dataFile);
            }
        }

        /// <summary>
        /// Create stock series from label file. Download label files once a month id download is true. Only labels are downloaded no data.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="download"></param>
        /// <returns></returns>
        private bool InitAbcGroup(ABCGroup config, bool download)
        {
            var destFolder = config.LabelOnly ? DataFolder + ABC_DAILY_CFG_GROUP_FOLDER : DataFolder + ABC_DAILY_CFG_FOLDER;
            string fileName = Path.Combine(destFolder, config.AbcGroup.ToString() + ".csv");

            if (download) // Download label files once a Month
            {
                var downloadMonth = File.Exists(fileName) ? File.GetLastWriteTime(fileName).Month : 0;
                if (DateTime.Today.Month != downloadMonth)
                {
                    AbcClient.DownloadLabel(fileName, config.AbcGroup);
                }
            }

            if (!config.LabelOnly)
            {
                InitFromLibelleFile(config, fileName);
                return true;
            }
            else
            {
                groupSeries.Add(config.Group, null);
            }
            if (config.Group == StockSerie.Groups.SRD || config.Group == StockSerie.Groups.SRD_LO)
            {
                InitSRDFromLibelleFile(fileName, config.Group);
            }

            return true;
        }

        private void DownloadGroupFromAbc(ABCGroup groupConfig)
        {
            var history = groupDownloadHistory.FirstOrDefault(h => h.Name == groupConfig.AbcGroup);
            if (history == null)
            {
                groupDownloadHistory.Add(history = new AbcGroupDownloadHistory(groupConfig.AbcGroup, new DateTime(ARCHIVE_START_YEAR, 1, 1), DateTime.MinValue));
            }

            if (DateTime.Now > history.NextDownload)
            {
                var startDate = history.NextDownload == DateTime.MinValue ? new DateTime(ARCHIVE_START_YEAR, 1, 1) : history.LastDownload.Date.AddDays(-3);
                if (DownloadMonthlyFileFromABC(DataFolder + ABC_TMP_FOLDER, startDate, DateTime.Today, groupConfig, false))
                {
                    history.LastDownload = DateTime.Now;
                    switch (groupConfig.Market)
                    {
                        case Market.XETRA:
                        case Market.EURONEXT:
                            if (DateTime.Now.TimeOfDay < marketDownloadTimes[groupConfig.Market])
                                history.NextDownload = DateTime.Today.Add(marketDownloadTimes[groupConfig.Market]);
                            else
                                history.NextDownload = DateTime.Today.AddDays(1).Add(marketDownloadTimes[groupConfig.Market]);

                            if (history.NextDownload.DayOfWeek == DayOfWeek.Saturday)
                                history.NextDownload = history.NextDownload.AddDays(2);
                            if (history.NextDownload.DayOfWeek == DayOfWeek.Sunday)
                                history.NextDownload = history.NextDownload.AddDays(1);

                            break;
                        case Market.NYSE:
                            history.NextDownload = DateTime.Today.AddDays(1).Add(marketDownloadTimes[groupConfig.Market]);

                            if (history.NextDownload.DayOfWeek == DayOfWeek.Saturday)
                                history.NextDownload = history.NextDownload.AddDays(2);
                            if (history.NextDownload.DayOfWeek == DayOfWeek.Sunday)
                                history.NextDownload = history.NextDownload.AddDays(1);
                            break;
                        case Market.MIXED:
                            if (DateTime.Now.TimeOfDay < marketDownloadTimes[Market.EURONEXT])
                                history.NextDownload = DateTime.Today.Add(marketDownloadTimes[Market.EURONEXT]);
                            else
                                history.NextDownload = DateTime.Today.AddDays(1).Add(marketDownloadTimes[Market.NYSE]);

                            if (history.NextDownload.DayOfWeek == DayOfWeek.Saturday)
                                history.NextDownload = history.NextDownload.AddDays(2);
                            if (history.NextDownload.DayOfWeek == DayOfWeek.Sunday)
                                history.NextDownload = history.NextDownload.AddDays(1);
                            break;

                        default:
                            history.NextDownload = DateTime.Today.AddDays(1).Add(marketDownloadTimes[Market.NYSE]);

                            if (history.NextDownload.DayOfWeek == DayOfWeek.Sunday)
                                history.NextDownload = history.NextDownload.AddDays(2);
                            if (history.NextDownload.DayOfWeek == DayOfWeek.Monday)
                                history.NextDownload = history.NextDownload.AddDays(1);
                            break;
                    }
                }
            }
        }

        private void DownloadMonthlyFileHistoryFromABC(string destFolder, DateTime startDate, ABCGroup group, bool useCache = true)
        {
            var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
            var endOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);

            var success = true;
            while (success && endOfMonth >= startDate)
            {
                NotifyProgress($"Downloading history data for {group.Group} from {startOfMonth.ToShortDateString()}");


                string fileName = destFolder + @"\" + group.AbcGroup + "_" + startOfMonth.Year + "_" + startOfMonth.Month.ToString("0#") + ".csv";
                if (success = AbcClient.DownloadData(fileName, startOfMonth, endOfMonth, group.AbcGroup, useCache))
                {
                    StockLog.Write($"{group.Group} from:{startOfMonth:yy_MM_dd} to:{endOfMonth:yy_MM_dd} success");
                }
                else
                {
                    StockLog.Write($"{group.Group} from:{startOfMonth:yy_MM_dd} to:{endOfMonth:yy_MM_dd} failed !!!");
                }

                endOfMonth = startOfMonth.AddDays(-1);
                startOfMonth = new DateTime(endOfMonth.Year, endOfMonth.Month, 1);
            }
        }

        private bool DownloadMonthlyFileFromABC(string destFolder, DateTime startDate, DateTime endDate, ABCGroup group, bool useCache = false)
        {
            bool success = true;

            NotifyProgress($"Downloading data for {group.Group} from {startDate.ToShortDateString()}");
            try
            {
                if (endDate - startDate >= new TimeSpan(31, 0, 0, 0))
                {
                    DownloadMonthlyFileHistoryFromABC(destFolder, startDate, group, true);
                    startDate = new DateTime(endDate.Year, endDate.Month, 1);
                }

                string fileName = destFolder + @"\" + group.AbcGroup + "_" + endDate.Year + "_" + endDate.Month.ToString("0#") + ".csv";
                if (startDate >= endDate)
                    return false;

                if (AbcClient.DownloadData(fileName, startDate, endDate, group.AbcGroup, useCache))
                {
                    StockLog.Write($"{group.Group} from:{startDate:yy_MM_dd} to:{endDate:yy_MM_dd} success");
                }
                else
                {
                    StockLog.Write($"{group.Group} from:{startDate:yy_MM_dd} to:{endDate:yy_MM_dd} failed !!!");
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
                MessageBox.Show(ex.Message, "Connection failed");
                success = false;
            }
            return success;
        }

        private void InitFromSector(SectorCode sector)
        {
            var longName = "_" + sector.Sector;
            if (!stockDictionary.ContainsKey(longName))
            {
                // Set SectorId to stock
                string fileName = DataFolder + ABC_DAILY_CFG_SECTOR_FOLDER + @"\" + sector.Code.ToString() + ".txt";
                if (File.Exists(fileName))
                {
                    using StreamReader sr = new StreamReader(fileName, true);
                    string line;
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] fields = line.Split(';');
                            var stockSerie = stockDictionary.Values.FirstOrDefault(s => s.ISIN == fields[0]);
                            if (stockSerie != null)
                            {
                                stockSerie.SectorId = sector.Code;
                            }
                        }
                    }
                }
            }
        }

        public static void CreateDirectories()
        {
            if (!Directory.Exists(Folders.AgendaFolder))
            {
                Directory.CreateDirectory(Folders.AgendaFolder);
            }
            if (!Directory.Exists(DataFolder + ABC_DAILY_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_DAILY_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_INTRADAY_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_DAILY_CFG_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_DAILY_CFG_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_DAILY_CFG_SECTOR_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_DAILY_CFG_SECTOR_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_DAILY_CFG_GROUP_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_DAILY_CFG_GROUP_FOLDER);
            }
            else
            {
                foreach (string file in Directory.GetFiles(DataFolder + ABC_INTRADAY_FOLDER))
                {
                    // Purge files at each start
                    File.Delete(file);
                }
            }
            if (!Directory.Exists(DataFolder + ABC_WEB_CACHE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_WEB_CACHE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_WEB_PROCESSED_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_WEB_PROCESSED_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_TMP_CACHE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_TMP_CACHE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + ABC_TMP_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ABC_TMP_FOLDER);
            }
            else
            {
                foreach (string file in Directory.GetFiles(DataFolder + ABC_TMP_FOLDER))
                {
                    // Purge files at each start
                    File.Delete(file);
                }
            }
        }
        private void InitFromLibelleFile(ABCGroup config, string fileName)
        {
            if (!File.Exists(fileName))
            {
                StockLog.Write("File does not exist");
                return;
            }

            using StreamReader sr = new StreamReader(fileName, true);
            string line;
            sr.ReadLine(); // Skip first line
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line) && IsinMatchGroup(config, line))
                {
                    string[] row = line.Split(';');
                    string stockName = row[1].ToUpper().Replace(",", " "); // .Replace(" - ", " ").Replace("-", " ").Replace("  ", " ");

                    var abcId = row[0];
                    var isin = abcId.Substring(0, 12);
                    if (excludeList.Contains(isin))
                        continue;

                    if (!stockDictionary.ContainsKey(stockName))
                    {
                        var existingInstrument = stockDictionary.Values.FirstOrDefault(s => s.ISIN == isin);
                        if (existingInstrument != null)
                        {
                            StockLog.Write($"Duplicate ISIN {isin}:{config.Group}:{stockName} already listed from {existingInstrument.StockGroup}:{existingInstrument.StockName}");
                            continue;
                        }

                        var abcSuffix = abcId.Length > 12 ? abcId[12] : 'p';
                        StockSerie stockSerie = new StockSerie(stockName, row[2], isin, config.Group, StockDataProvider.ABC, BarDuration.Daily)
                        {
                            AbcId = abcId,
                            ABCName = row[2] + abcSuffix
                        };

                        stockDictionary.Add(stockName, stockSerie);
                    }
                    else
                    {
                        StockLog.Write("Duplicate " + config.Group + ";" + line + " already in group " + stockDictionary[stockName].StockGroup);
                    }
                }
            }
        }
        private void InitSRDFromLibelleFile(string fileName, Groups group)
        {
            if (File.Exists(fileName))
            {
                using StreamReader sr = new StreamReader(fileName, true);
                string line;
                sr.ReadLine(); // Skip first line
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                    {
                        string[] row = line.Split(';');
                        string stockName = row[1].ToUpper().Replace(" - ", " ").Replace("-", " ").Replace("  ", " ");
                        if (stockDictionary.ContainsKey(stockName))
                        {
                            if (group == StockSerie.Groups.SRD)
                            {
                                stockDictionary[stockName].SRD = true;
                            }
                            if (group == StockSerie.Groups.SRD_LO)
                            {
                                stockDictionary[stockName].SRD_LO = true;
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

        private bool IsinMatchGroup(ABCGroup group, string line)
        {
            if (group.Prefixes != null && group.Prefixes.Length > 0)
            {
                return group.Prefixes.Any(prefix => line.StartsWith(prefix));
            }
            return true;
        }

        private void InitFromFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                using StreamReader sr = new StreamReader(fileName, true);
                string line;
                sr.ReadLine(); // Skip first line
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                    {
                        string[] row = line.Split(';');
                        if (excludeList.Contains(row[0]))
                            continue;

                        StockSerie stockSerie = new StockSerie(row[1], string.IsNullOrEmpty(row[2]) ? row[3] : row[2], row[0], (Groups)Enum.Parse(typeof(Groups), row[4]), StockDataProvider.ABC, BarDuration.Daily);
                        stockSerie.ABCName = stockSerie.Symbol + stockSerie.ISIN?.Substring(0, 2) switch
                        {
                            null => string.Empty,
                            "FR" => "p",
                            "QS" => "p",
                            "BE" => "g",
                            "NL" => "n",
                            "DE" => "f",
                            "IT" => "i",
                            "ES" => "m",
                            "PT" => "I",
                            _ => string.Empty
                        };
                        if (!stockDictionary.ContainsKey(row[1]))
                        {
                            stockDictionary.Add(row[1], stockSerie);
                        }
                        else
                        {
                            StockLog.Write("ABC Entry: " + row[1] + " already in stockDictionary");
                        }
                    }
                }
            }
        }

        private void DownloadFromConfigFile(string fileName)
        {
            if (!File.Exists(fileName))
                return;
            using StreamReader sr = new StreamReader(fileName, true);
            string line;
            sr.ReadLine(); // Skip first line
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine();
                if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                {
                    if (stockDictionary.TryGetValue(line.Split(';')[1], out var stockSerie))
                    {
                        this.DownloadABCData(stockSerie);
                    }
                    else
                    {
                        StockLog.Write($"Serie from config file not found: {line}");
                    }
                }
            }
        }

        static string loadingGroup = null;

        public override bool LoadData(StockSerie stockSerie)
        {
            bool res = false;
            StockLog.Write("Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName + " - " + stockSerie.Count);

            // Read from CSV files Archive + current year.
            if (stockSerie.Count == 0)
            {
                res = this.LoadFromCSV(stockSerie);
            }

            // Load data that just has been downloaded
            string abcGroup = GetABCGroup(stockSerie.StockGroup);
            if (abcGroup != null)
            {
                // Group data is available only after download
                LoadGroupData(abcGroup, stockSerie.StockGroup);
            }
            else
            {
                // Daily value is available only after download
                string fileName = stockSerie.ISIN + "_" + stockSerie.Symbol + "_" + stockSerie.StockGroup.ToString() + ".csv";
                res |= ParseCSVFile(stockSerie, Path.Combine(DataFolder + ABC_TMP_FOLDER, fileName));
            }

            this.ApplySplit(stockSerie);

            return res;
        }
        private void LoadGroupData(string abcGroup, Groups stockGroup)
        {
            StockLog.Write("Group: " + abcGroup);
            try
            {
                #region Multi thread synchro
                if (loadingGroup == null)
                {
                    loadingGroup = abcGroup;
                }
                else
                {
                    StockLog.Write("Already busy loading group: " + loadingGroup);
                    if (loadingGroup == abcGroup)
                    {
                        do
                        {
                            Thread.Sleep(100);
                        } while (loadingGroup == abcGroup);
                        return;
                    }
                    else
                    {
                        do
                        {
                            Thread.Sleep(100);
                        } while (loadingGroup == abcGroup);
                    }
                }
                #endregion

                string filePattern = abcGroup + "_*.csv";
                bool dataLoaded = false;
                foreach (string currentFile in Directory.GetFiles(DataFolder + ABC_TMP_FOLDER, filePattern).OrderBy(s => s))
                {
                    NotifyProgress($"Loading {Path.GetFileNameWithoutExtension(currentFile)}");
                    ParseABCGroupCSVFile(currentFile, stockGroup);
                    File.Delete(currentFile);
                    dataLoaded = true;
                }
                var fileName = Path.Combine(DataFolder + ABC_INTRADAY_FOLDER, abcGroup + ".csv");
                if (File.Exists(fileName))
                {
                    ParseABCGroupCSVFile(fileName, stockGroup, true);
                    File.Delete(fileName);
                    dataLoaded = true;
                }
                // Save to CSV file
                if (dataLoaded)
                {
                    NotifyProgress($"Saving files for {stockGroup}");
                    foreach (var serie in stockDictionary.Values.Where(s => s.BelongsToGroup(stockGroup) && s.Count > 0))
                    {
                        this.SaveToCSV(serie, false);
                        this.ApplySplit(serie);
                    }
                }
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            finally
            {
                loadingGroup = null;
            }
        }
        private static string GetABCGroup(Groups stockGroup)
        {
            var groupConfig = abcGroupConfig.FirstOrDefault(g => g.Group == stockGroup);
            //if (groupConfig == null)
            //{
            //    MessageBox.Show($"Group {stockGroup} not defined in configuration file", "Download configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    return null;
            //}

            return groupConfig?.AbcGroup;


            //string abcGroup = null;
            //switch (stockGroup)
            //{
            //    case StockSerie.Groups.EURO_A:
            //        abcGroup = "eurolistap";
            //        break;
            //    case StockSerie.Groups.EURO_B:
            //        abcGroup = "eurolistbp";
            //        break;
            //    case StockSerie.Groups.EURO_C:
            //        abcGroup = "eurolistcp";
            //        break;
            //    case StockSerie.Groups.ALTERNEXT:
            //        abcGroup = "eurogp";
            //        break;
            //    case StockSerie.Groups.SECTORS_CAC:
            //        abcGroup = "indicessecp";
            //        break;
            //    case StockSerie.Groups.BELGIUM:
            //        abcGroup = "belg";
            //        break;
            //    case StockSerie.Groups.HOLLAND:
            //        abcGroup = "holln";
            //        break;
            //    case StockSerie.Groups.PORTUGAL:
            //        abcGroup = "lisboal";
            //        break;
            //    case StockSerie.Groups.CAC40:
            //        abcGroup = "xcac40p";
            //        break;
            //    case StockSerie.Groups.SBF120:
            //        abcGroup = "xsbf120p";
            //        break;
            //    case StockSerie.Groups.CAC_AT:
            //        abcGroup = "xcacatp";
            //        break;
            //    case StockSerie.Groups.USA:
            //        abcGroup = "usau";
            //        break;
            //    case StockSerie.Groups.SPAIN:
            //        abcGroup = "spainm";
            //        break;
            //    case StockSerie.Groups.ITALIA:
            //        abcGroup = "italiai";
            //        break;
            //    case StockSerie.Groups.GERMANY:
            //        abcGroup = "germanyf";
            //        break;
            //    //case StockSerie.Groups.NASDAQ:
            //    //    abcGroup = "nasu";
            //    //    break;
            //    case StockSerie.Groups.SRD:
            //        abcGroup = "srdp";
            //        break;
            //    case StockSerie.Groups.SRD_LO:
            //        abcGroup = "srdlop";
            //        break;
            //    default:
            //        StockLog.Write($"StockGroup {stockGroup} is not supported in ABC Bourse");
            //        break;
            //}
            //return abcGroup;
        }
        private bool ParseABCGroupCSVFile(string fileName, Groups group, bool intraday = false)
        {
            if (!File.Exists(fileName)) return false;
            StockSerie stockSerie = null;
            using StreamReader sr = new StreamReader(fileName, true);
            string previousISIN = string.Empty;
            DateTime date = File.GetLastWriteTime(fileName);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine().Replace(",", ".");
                try
                {
                    string[] row = line.Split(';');
                    if (previousISIN != row[0])
                    {
                        stockSerie = stockDictionary.Values.FirstOrDefault(s => s.ISIN == row[0] && s.StockGroup == group);
                        if (stockSerie == null)
                            continue;
                        previousISIN = row[0];
                        if (stockSerie.Count == 0)
                        {
                            this.LoadFromCSV(stockSerie);
                        }
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
                catch (Exception e)
                {
                    StockLog.Write(line);
                    StockLog.Write(e);
                    return false;
                }
            }
            return true;
        }
        public override bool ForceDownloadData(StockSerie stockSerie)
        {
            string filePattern = stockSerie.ISIN + "_" + stockSerie.Symbol + "_" + stockSerie.StockGroup.ToString() + "_*.csv";
            string fileName;
            StockLog.Write(stockSerie.StockName + " " + stockSerie.ISIN);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                StockLog.Write("Network is Available");
                stockSerie.IsInitialised = false;
                int nbFile = 0;
                var isin = stockSerie.ISIN;
                if (stockSerie.StockGroup == StockSerie.Groups.USA)
                    isin += "u";
                for (int i = DateTime.Today.Year - 1; i >= ARCHIVE_START_YEAR; i--)
                {
                    fileName = filePattern.Replace("*", i.ToString());
                    if (!AbcClient.DownloadIsin(Path.Combine(DataFolder + ABC_TMP_FOLDER, fileName), new DateTime(i, 1, 1), new DateTime(i, 12, 31), isin))
                    {
                        break;
                    }
                    nbFile++;
                }
                int year = DateTime.Today.Year;
                fileName = filePattern.Replace("*", year.ToString());
                if (AbcClient.DownloadIsin(Path.Combine(DataFolder + ABC_TMP_FOLDER, fileName), new DateTime(year, 1, 1), DateTime.Today, isin))
                {
                    nbFile++;
                }
                if (nbFile == 0)
                    return false;

                // Parse loaded files
                foreach (var csvFileName in Directory.GetFiles(DataFolder + ABC_TMP_FOLDER, filePattern).OrderBy(f => f))
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
                return true;
            }
            else
            {
                StockLog.Write("Network is not Available");
                return false;
            }
        }

        DateTime lastDownloadedCAC40Date = DateTime.MaxValue;
        bool happyNewMonth = false;
        public bool DownloadABCData(StockSerie stockSerie)
        {
            StockLog.Write("DownloadABCData Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return false;

            var history = downloadHistory.FirstOrDefault(h => h.Id == stockSerie.ISIN);
            if (history == null)
            {
                NotifyProgress($"Downloading {stockSerie.StockName}");
                if (!ForceDownloadData(stockSerie) || stockSerie.Count == 0)
                    return false;

                history = new AbcDownloadHistory(stockSerie.ISIN, stockSerie.Keys.Last(), stockSerie.StockName, stockSerie.StockGroup.ToString());
                downloadHistory.Add(history);

                return true;
            }

            // Check if up to date
            var lastLoadedDate = history.LastDate;
            if (stockSerie.StockName != "CAC40" && lastLoadedDate >= lastDownloadedCAC40Date)
                return true;

            NotifyProgress($"Downloading {stockSerie.StockName}");

            var fileName = stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv";
            var needDownload = NeedDownload(stockSerie, history);
            if (needDownload && AbcClient.DownloadIsin(Path.Combine(DataFolder + ABC_TMP_FOLDER, fileName), lastLoadedDate.AddDays(1), DateTime.Today, stockSerie.ISIN))
            {
                stockSerie.IsInitialised = false;

                this.LoadData(stockSerie);

                if (stockSerie.StockName == "CAC40")
                {
                    lastDownloadedCAC40Date = stockSerie.Keys.Last();
                    happyNewMonth = lastDownloadedCAC40Date.Month != DateTime.Today.Month || lastDownloadedCAC40Date.Month != lastLoadedDate.Month;
                }

                this.SaveToCSV(stockSerie, happyNewMonth);
                File.Delete(Path.Combine(DataFolder + ABC_TMP_FOLDER, fileName));

                history.LastDate = stockSerie.Keys.Last();
            }
            else // Failed loading data, could be because data is up to date.
            {
                if (stockSerie.StockName == "CAC40")
                {
                    lastDownloadedCAC40Date = history.LastDate;
                    happyNewMonth = false;
                }
            }

            return true;
        }

        static TimeSpan euronextClose = new TimeSpan(18, 0, 0);
        static TimeSpan xetraClose = new TimeSpan(21, 0, 0);

        private bool NeedDownload(StockSerie stockSerie, AbcDownloadHistory history)
        {
            var today = DateTime.Today;
            if (history.LastDate == today)
                return false;
            if (today.DayOfWeek == DayOfWeek.Saturday && history.LastDate == today.AddDays(-1))
                return false;
            if (today.DayOfWeek == DayOfWeek.Sunday && history.LastDate == today.AddDays(-2))
                return false;
            if ((today - history.LastDate).TotalDays > 1)
                return true;
            var now = DateTime.Now;
            if (now.TimeOfDay < euronextClose)
                return false;
            if (stockSerie.Market == 'p' || stockSerie.Market == 'g' || stockSerie.Market == 'n' || stockSerie.Market == 'l')
                return true;
            if (now.TimeOfDay < xetraClose)
                return false;
            if (stockSerie.Market == 'f' || stockSerie.Market == 'i' || stockSerie.Market == 'm')
                return true;
            return false;
        }

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            StockLog.Write("Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                this.LoadData(stockSerie);
                if (stockSerie.Count == 0)
                {
                    return ForceDownloadData(stockSerie);
                }
            }

            return stockSerie.Initialise();
        }
        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            return false;
            //StockLog.Write("DownloadIntradayData Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

            //if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            //{
            //    NotifyProgress("Downloading intraday for" + stockSerie.StockGroup.ToString());

            //    if (!stockSerie.Initialise() || stockSerie.Count == 0)
            //    {
            //        return false;
            //    }

            //    if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday || stockSerie.Keys.Last().Date == DateTime.Today)
            //    {
            //        return false;
            //    }

            //    string abcGroup = GetABCGroup(stockSerie.StockGroup);
            //    if (abcGroup != null)
            //    {
            //        var destFolder = DataFolder + ABC_INTRADAY_FOLDER;
            //        string fileName = abcGroup + ".csv";
            //        if (this.DownloadIntradayGroup(destFolder, fileName, abcGroup))
            //        {
            //            // Deinitialise all the stocks belonging to group
            //            foreach (StockSerie serie in stockDictionary.Values.Where(s => s.BelongsToGroup(stockSerie.StockGroup)))
            //            {
            //                serie.IsInitialised = false;
            //                serie.ClearBarDurationCache();
            //            }
            //        }
            //    }
            //}
            //return true;
        }

        private String downloadingGroups = String.Empty;
        TimeSpan nextDownload = new TimeSpan(9, 1, 0);
        public void DownloadAllGroupsIntraday()
        {
            return;
            //StockLog.Write(string.Empty);
            //try
            //{
            //    if (IntradayDownloadSuspended)
            //        return;
            //    var now = DateTime.Now.TimeOfDay;
            //    if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday || DateTime.Today.DayOfWeek == DayOfWeek.Saturday || now.Hours > 18 || now.Hours < 9)
            //        return;
            //    while (downloadingGroups != String.Empty)
            //    {
            //        Thread.Sleep(500);
            //    }
            //    lock (downloadingGroups)
            //    {
            //        // Download ABC intraday data
            //        if (now < nextDownload)
            //        {
            //            StockLog.Write("Up To Date");
            //            return;
            //        }
            //        int minutes = ((int)now.TotalMinutes / 5) * 5;
            //        nextDownload = TimeSpan.FromMinutes(minutes + 5);

            //        downloadingGroups = "True";
            //        var groups = new Groups[] {
            //            StockSerie.Groups.BELGIUM, StockSerie.Groups.HOLLAND, StockSerie.Groups.PORTUGAL,
            //            StockSerie.Groups.ITALIA, StockSerie.Groups.GERMANY, StockSerie.Groups.SPAIN, StockSerie.Groups.USA, StockSerie.Groups.CANADA,
            //            StockSerie.Groups.EURO_A, StockSerie.Groups.EURO_B, StockSerie.Groups.EURO_C, StockSerie.Groups.ALTERNEXT };

            //        foreach (var group in groups)
            //        {
            //            string abcGroup = GetABCGroup(group);
            //            if (abcGroup != null)
            //            {
            //                var destFolder = DataFolder + ABC_INTRADAY_FOLDER;
            //                string fileName = abcGroup + ".csv";
            //                if (this.DownloadIntradayGroup(destFolder, fileName, abcGroup))
            //                {
            //                    // Deinitialise all the stocks belonging to group
            //                    foreach (StockSerie serie in stockDictionary.Values.Where(s => s.BelongsToGroup(group)))
            //                    {
            //                        using (new StockSerieLocker(serie))
            //                        {
            //                            serie.IsInitialised = false;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    StockLog.Write(ex.Message);
            //}
            //finally
            //{
            //    downloadingGroups = String.Empty;
            //}
        }

        /// <summary>
        /// Parse ABC file
        /// </summary>
        /// <param name="stockSerie"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected override bool ParseCSVFile(StockSerie stockSerie, string fileName)
        {
            if (File.Exists(fileName))
            {
                using StreamReader sr = new StreamReader(fileName);
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

        public static List<SectorCode> SectorCodes = new List<SectorCode>
        {
                new SectorCode(45,"Biens de consommation"),
                new SectorCode(50,"Industries"),
                new SectorCode(35,"Immobilier"),
                new SectorCode(55,"Materiaux de base"),
                new SectorCode(60,"Petrole et gaz"),
                new SectorCode(20,"Sante"),
                new SectorCode(40,"Services aux consommateurs"),
                new SectorCode(65,"Services aux collectivites"),
                new SectorCode(30,"Societes financieres"),
                new SectorCode(15,"Telecommunications"),
                new SectorCode(10,"Technologie")
        };
        private bool DownloadSectorFromABC(string destFolder, int sector)
        {
            return false;
            //bool success = true;

            //string fileName = destFolder + @"\" + sector + ".txt";
            //if (File.Exists(fileName))
            //{
            //    if (File.GetLastWriteTime(fileName) > DateTime.Now.AddDays(-7)) // File has been updated during the last 7 days
            //        return true;
            //}
            //if (!this.Initialize())
            //    return false;

            //try
            //{
            //    // Send POST request
            //    string url = $"/api/General/DownloadSector?sectorCode={sector}";
            //    var resp = httpClient.GetAsync(url).Result;
            //    if (!resp.IsSuccessStatusCode)
            //        return false;
            //    using var respStream = resp.Content.ReadAsStreamAsync().GetAwaiter().GetResult();
            //    using var fileStream = File.Create(Path.Combine(destFolder, fileName));
            //    respStream.CopyTo(fileStream);
            //}
            //catch (Exception ex)
            //{
            //    StockLog.Write(ex);
            //    System.Windows.Forms.MessageBox.Show(ex.Message, "Connection failed loading sectors");
            //    success = false;
            //}
            //return success;
        }

        private bool SaveResponseToFile(string fileName, HttpWebRequest req)
        {
            bool success = true;
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                // Get the stream containing content returned by the server.
                using Stream dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                using StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();

                if (responseFromServer.Length != 0 && !responseFromServer.StartsWith("<", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Save content to file
                    using StreamWriter writer = new StreamWriter(fileName);
                    writer.Write(responseFromServer);
                    //StockLog.Write("Download succeeded: " + fileName);
                }
                else
                {
                    success = false;
                }
            }
            return success;
        }

        #region IConfigDialog Implementation
        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            EuronextDataProviderConfigDlg configDlg = new EuronextDataProviderConfigDlg(stockDico) { StartPosition = FormStartPosition.CenterScreen };
            return configDlg.ShowDialog();
        }

        public override string DisplayName => "ABCBourse";
        #endregion

        static SortedDictionary<Groups, List<string>> groupSeries = new SortedDictionary<Groups, List<string>>();

        public static bool BelongsToGroup(StockSerie stockSerie, Groups group)
        {
            if (group == stockSerie.StockGroup || group == StockSerie.Groups.ALL_STOCKS)
                return true;

            switch (group)
            {
                case Groups.EURO_A_B:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B;
                case Groups.EURO_A_B_C:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B || stockSerie.StockGroup == Groups.EURO_C;
                case Groups.CACALL:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B || stockSerie.StockGroup == Groups.EURO_C || stockSerie.StockGroup == Groups.ALTERNEXT;
                case Groups.PEA_EURONEXT:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B || stockSerie.StockGroup == Groups.EURO_C || stockSerie.StockGroup == Groups.ALTERNEXT
                        || stockSerie.StockGroup == Groups.BELGIUM || stockSerie.StockGroup == Groups.HOLLAND || stockSerie.StockGroup == Groups.PORTUGAL;
                case Groups.PEA:
                    return stockSerie.StockGroup == Groups.EURO_A || stockSerie.StockGroup == Groups.EURO_B || stockSerie.StockGroup == Groups.EURO_C || stockSerie.StockGroup == Groups.ALTERNEXT
                        || stockSerie.StockGroup == Groups.BELGIUM || stockSerie.StockGroup == Groups.HOLLAND || stockSerie.StockGroup == Groups.PORTUGAL
                        || stockSerie.StockGroup == Groups.ITALIA || stockSerie.StockGroup == Groups.GERMANY || stockSerie.StockGroup == Groups.SPAIN;
            }

            if (!groupSeries.ContainsKey(group))
                return false;

            var groupList = groupSeries[group];
            if (groupList == null)
            {
                groupList = new List<string>();
                var abcGroup = abcGroupConfig.FirstOrDefault(g => g.Group == group);
                // parse group definition
                string fileName = DataFolder + @"\" + ABC_DAILY_CFG_GROUP_FOLDER + $@"\{abcGroup.AbcGroup}.csv";
                if (File.Exists(fileName))
                {
                    using StreamReader sr = new StreamReader(fileName, true);
                    string line;
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] row = line.Split(';');
                            groupList.Add(row[1].ToUpper());
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"Group definition file not found for Group: {group}", "ABD DataProvider Group error");
                }
            }

            return groupList != null && groupList.Contains(stockSerie.StockName);
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

            string url = $"http://www.abcbourse.com/marches/events.aspx?s={stockSerie.ABCName}";

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

                if (!stockSerie.Agenda.ContainsKey(date))
                {
                    stockSerie.Agenda.Add(date, row[1], row[2]);
                }
            }
            stockSerie.Agenda.DownloadDate = DateTime.Today;
            stockSerie.Agenda.SortDescending();
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

        #region Persistency
        const string DATEFORMAT = "dd/MM/yyyy";
        public void SaveToCSV(StockSerie stockSerie, bool forceArchive = true)
        {
            if (stockSerie.Values.Count() == 0)
                return;
            string fileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            var lastDate = stockSerie.Keys.Last();
            var pivotDate = new DateTime(lastDate.Year, lastDate.Month, 1).AddDays(-1);
            if (forceArchive || !File.Exists(fileName))
            {
                if (stockSerie.Values.Any(v => v.DATE <= pivotDate))
                {
                    using StreamWriter sw = new StreamWriter(fileName);
                    foreach (var value in stockSerie.Values.Where(v => v.DATE <= pivotDate))
                    {
                        sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
                    }
                }
            }

            fileName = Path.Combine(DataFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                foreach (var value in stockSerie.Values.Where(v => v.DATE > pivotDate && v.IsComplete))
                {
                    sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
                }
            }
        }

        public DateTime GetLastDate(StockSerie stockSerie)
        {
            DateTime date = DateTime.MinValue;
            string fileName = Path.Combine(DataFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            if (File.Exists(fileName))
            {
                var lines = File.ReadAllLines(fileName);
                if (lines.Length > 0)
                {
                    string[] row = lines.Last().Split(';');
                    return DateTime.ParseExact(row[0], DATEFORMAT, usCulture);
                }
            }
            fileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            if (File.Exists(fileName))
            {
                var line = ReadLastLine(fileName);
                if (!string.IsNullOrEmpty(line))
                {
                    string[] row = line.Split(';');
                    return DateTime.ParseExact(row[0], DATEFORMAT, usCulture);
                }
            }
            return date;
        }


        public static string ReadLastLine(string filePath)
        {
            var encoding = Encoding.UTF8;

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long position = fs.Length - 1;
            int byteRead;
            var lineBytes = new List<byte>();

            while (position >= 0)
            {
                fs.Seek(position, SeekOrigin.Begin);
                byteRead = fs.ReadByte();

                if (byteRead == '\n' && lineBytes.Count > 0)
                    break;

                lineBytes.Insert(0, (byte)byteRead);
                position--;
            }

            return encoding.GetString(lineBytes.ToArray()).TrimEnd('\r', '\n');
        }



        public bool LoadFromCSV(StockSerie stockSerie, bool loadArchive = true)
        {
            StockLog.Write($"Serie: {stockSerie.StockName}");
            bool result = false;
            string fileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            var lastArchiveDate = DateTime.MinValue;
            if (loadArchive && File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] row = sr.ReadLine().Split(';');
                        DateTime date = DateTime.ParseExact(row[0], DATEFORMAT, usCulture);
                        var value = new StockDailyValue(float.Parse(row[1], usCulture), float.Parse(row[2], usCulture), float.Parse(row[3], usCulture), float.Parse(row[4], usCulture), long.Parse(row[5]), date);
                        stockSerie.Add(value.DATE, value);
                    }
                }
                result = true;
                if (stockSerie.Count > 0)
                    lastArchiveDate = stockSerie.Keys.Last();
            }

            fileName = Path.Combine(DataFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            if (File.Exists(fileName))
            {
                DateTime date = DateTime.MinValue;
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] row = sr.ReadLine().Split(';');
                        date = DateTime.ParseExact(row[0], DATEFORMAT, usCulture);
                        if (date > lastArchiveDate)
                        {
                            var value = new StockDailyValue(float.Parse(row[1], usCulture), float.Parse(row[2], usCulture), float.Parse(row[3], usCulture), float.Parse(row[4], usCulture), long.Parse(row[5]), date);
                            stockSerie.Add(value.DATE, value);
                        }
                    }
                }
                result = true;
            }
            fileName = Path.Combine(DataFolder + ABC_TMP_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            if (File.Exists(fileName))
            {
                bool needSave = false;
                DateTime lastDate = stockSerie.Keys.Last();
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (!sr.EndOfStream)
                    {
                        string[] row = sr.ReadLine().Split(';');
                        var date = DateTime.ParseExact(row[1], "dd/MM/yy", frenchCulture);
                        if (date > lastDate)
                        {
                            needSave = true;
                            lastDate = date;
                            var value = new StockDailyValue(float.Parse(row[2], frenchCulture), float.Parse(row[3], frenchCulture), float.Parse(row[4], frenchCulture), float.Parse(row[5], frenchCulture), long.Parse(row[6]), date);
                            stockSerie.Add(value.DATE, value);
                        }
                    }
                }
                File.Delete(fileName);

                if (loadArchive && needSave)
                {
                    SaveToCSV(stockSerie, lastArchiveDate < new DateTime(lastDate.Year, lastDate.Month, 1).AddMonths(-1));
                }
                result = true;
            }
            return result;
        }
        #endregion

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            if (stockSerie.ABCName != null)
            {
                string url = $"https://www.abcbourse.com/graphes/display.aspx?s={stockSerie.ABCName}";
                Process.Start(url);
            }
        }

        public override void AddSplit(StockSerie stockSerie, DateTime date, float before, float after)
        {
            if (!stockSerie.Initialise())
                return;

            var split = new StockSplit() { StockName = stockSerie.StockName, Date = date, Before = before, After = after };
            StockSplit.Splits.Add(split);
            StockSplit.Save();

            var barDuration = stockSerie.BarDuration;
            stockSerie.BarDuration = BarDuration.Daily;

            stockSerie.IsInitialised = false;
            stockSerie.BarDuration = barDuration;
        }

        private void ApplySplit(StockSerie stockSerie)
        {
            if (stockSerie.Count == 0)
                return;

            foreach (var split in StockSplit.Splits.Where(s => s.StockName == stockSerie.StockName).OrderBy(s => s.Date))
            {
                float ratio = split.Before / split.After;
                foreach (var value in stockSerie.Values.Where(v => v.DATE < split.Date))
                {
                    value.ApplyRatio(ratio);
                }
            }
        }
        public virtual void ApplyTrimAfter(DateTime endDate)
        {
            if (File.Exists(historyFileName))
                File.Delete(historyFileName);
            this.downloadHistory = new List<AbcDownloadHistory>();

            foreach (var groupHistory in groupDownloadHistory)
            {
                groupHistory.LastDownload = endDate;
            }
            AbcGroupDownloadHistory.Save(groupHistoryFileName, groupDownloadHistory);

            // Clean Data
            foreach (var stockSerie in stockDictionary.Values.Where(s => s.DataProvider == StockDataProvider.ABC))
            {
                // Delete non archive file
                var fileName = Path.Combine(DataFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                if (!stockSerie.Initialise())
                    continue;

                stockSerie.BarDuration = BarDuration.Daily;
                if (stockSerie.LastValue.DATE < endDate)
                    continue;

                // Trim archive
                fileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
                using StreamWriter sw = new StreamWriter(fileName);
                foreach (var value in stockSerie.Values.Where(v => v.DATE < endDate))
                {
                    sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
                }

                stockSerie.IsInitialised = false;
            }
        }

        public override void ApplyTrimBefore(StockSerie stockSerie, DateTime date)
        {
            // Delete non archive file
            var fileName = Path.Combine(DataFolder + ABC_DAILY_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            if (!stockSerie.Initialise())
                return;

            stockSerie.BarDuration = BarDuration.Daily;
            if (stockSerie.LastValue.DATE < date)
                return;

            // Trim archive
            fileName = Path.Combine(DataFolder + ARCHIVE_FOLDER, stockSerie.ISIN + "_" + stockSerie.Symbol + ".csv");
            using StreamWriter sw = new StreamWriter(fileName);
            foreach (var value in stockSerie.Values.Where(v => v.DATE > date))
            {
                sw.WriteLine(value.DATE.ToString(DATEFORMAT) + ";" + value.OPEN.ToString(usCulture) + ";" + value.HIGH.ToString(usCulture) + ";" + value.LOW.ToString(usCulture) + ";" + value.CLOSE.ToString(usCulture) + ";" + value.VOLUME.ToString(usCulture));
            }

            stockSerie.IsInitialised = false;
        }
        public static void AddToExcludedList(IEnumerable<string> isins)
        {
            foreach (var isin in isins)
            {
                if (!string.IsNullOrEmpty(isin) && !excludeList.Contains(isin))
                {
                    excludeList.Add(isin);
                }
            }
            File.WriteAllLines(excludeFileName, excludeList);
        }
        public override bool RemoveEntry(StockSerie stockSerie)
        {
            if (!string.IsNullOrEmpty(stockSerie.ISIN) && !excludeList.Contains(stockSerie.ISIN))
            {
                excludeList.Add(stockSerie.ISIN);
                File.WriteAllLines(excludeFileName, excludeList);
            }
            return false;
        }
    }
}