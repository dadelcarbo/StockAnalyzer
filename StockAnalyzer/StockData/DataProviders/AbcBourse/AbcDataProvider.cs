using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockAnalyzer.StockData.DataProviders.AbcBourse
{
    public class AbcDataProvider : DataProviderBase
    {
        public override string DisplayName => "ABC Bourse";

        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.Daily, BarDuration.Weekly, BarDuration.Monthly };
        public override BarDuration DefaultDuration => BarDuration.Daily;
        public override DataProvider Provider => DataProvider.ABC;

        #region Exclude list
        static string excludeFileName = Path.Combine(Folders.PersonalFolder, "AbcExclude.txt");
        static List<string> excludeList = new List<string>();

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
        #endregion

        string ABC_WEB_CACHE_FOLDER;
        string ABC_TMP_FOLDER;
        string ABC_PROCESSED_FOLDER;
        string ABC_DAILY_CFG_GROUP_FOLDER;
        string ABC_DAILY_CFG_FOLDER;
        string configPath;

        protected override void PreInitDictionary(bool download)
        {
            ABC_WEB_CACHE_FOLDER = Path.Combine(DataRootFolder, "WebCache");
            if (!Directory.Exists(ABC_WEB_CACHE_FOLDER))
            {
                Directory.CreateDirectory(ABC_WEB_CACHE_FOLDER);
            }
            AbcClient.CacheFolder = ABC_WEB_CACHE_FOLDER;

            ABC_PROCESSED_FOLDER = Path.Combine(DataRootFolder, @"Processed");
            if (!Directory.Exists(ABC_PROCESSED_FOLDER))
            {
                Directory.CreateDirectory(ABC_PROCESSED_FOLDER);
            }

            ABC_TMP_FOLDER = Path.Combine(DataRootFolder, "Tmp");
            if (!Directory.Exists(ABC_TMP_FOLDER))
            {
                Directory.CreateDirectory(ABC_TMP_FOLDER);
            }
            else
            {
                foreach (string file in Directory.GetFiles(ABC_TMP_FOLDER))
                {
                    // Purge files at each start
                    File.Delete(file);
                }
            }
            ABC_DAILY_CFG_GROUP_FOLDER = Path.Combine(DataRootFolder, @"lbl\group");
            if (!Directory.Exists(ABC_DAILY_CFG_GROUP_FOLDER))
            {
                Directory.CreateDirectory(ABC_DAILY_CFG_GROUP_FOLDER);
            }

            ABC_DAILY_CFG_FOLDER = Path.Combine(DataRootFolder, @"lbl\cfg");
            if (!Directory.Exists(ABC_DAILY_CFG_FOLDER))
            {
                Directory.CreateDirectory(ABC_DAILY_CFG_FOLDER);
            }

            if (File.Exists(excludeFileName))
            {
                excludeList = File.ReadAllLines(excludeFileName).ToList();
            }

            configPath = Path.Combine(Folders.PersonalFolder, "AbcDownloadConfig.txt");
            groupHistoryFileName = Path.Combine(DataRootFolder, "GroupDownloadHistory.txt");
        }
        /// <summary>
        /// Creates a StockInstrument from a configuration line. Format: FR0003500008;CAC40;;PX1;INDICES
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            try
            {
                string[] row = line.Split(';');

                if (excludeList.Contains(row[0]))
                    return null;

                var id = row[0];
                var abcSuffix = id.Length > 12 ? id[12] : 'p';
                var instrument = new StockInstrument()
                {
                    Id = id,
                    Name = row[1],
                    Isin = id.Substring(0, 12),
                    Symbol = row[3],
                    AbcSuffix = abcSuffix,
                    Provider = DataProvider.ABC,
                    Group = (Groups)Enum.Parse(typeof(Groups), row[4]),
                    Market = Market.EURONEXT
                };

                return instrument;
            }
            catch (Exception ex)
            {
                StockLog.Write($"Line: {line}");
                StockLog.Write(ex.Message);
            }
            return null;
        }

        static string spiricaCsv => "spirica.csv";
        static List<ABCGroup> abcGroupConfig = null;
        protected override void PostInitDictionary(bool download)
        {
            // Copy spirica file if newer
            var dest = Path.Combine(ABC_DAILY_CFG_GROUP_FOLDER, spiricaCsv);
            var source = Path.Combine(Folders.PersonalFolder, spiricaCsv);
            if (!File.Exists(dest) || File.GetLastWriteTime(source) > File.GetLastWriteTime(dest))
            {
                File.Copy(source, dest, true);
            }

            if (!File.Exists(configPath))
            {
                MessageBox.Show("The default ABC configuration file is missing in:" + Environment.NewLine + configPath, "ABC Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            abcGroupConfig = JsonSerializer.Deserialize<List<ABCGroup>>(File.ReadAllText(configPath), new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } });

            #region Create Series - no Download (but lalels)

            // Intialize Groups
            foreach (var config in abcGroupConfig)
            {
                InitAbcGroup(config, true);
            }
            #endregion

            groupDownloadHistory = AbcGroupDownloadHistory.Load(groupHistoryFileName);

            if (download)
            {
                try
                {
                    // Download groups
                    foreach (var groupConfig in abcGroupConfig.Where(c => !c.LabelOnly))
                    {
                        NotifyProgress($"Downloading data for {groupConfig.Group}");

                        var history = groupDownloadHistory.FirstOrDefault(h => h.Name == groupConfig.AbcGroup);
                        if (history == null)
                        {
                            groupDownloadHistory.Add(history = new AbcGroupDownloadHistory(groupConfig.AbcGroup, DateTime.MinValue, DateTime.MinValue));
                        }

                        this.DownloadGroupFromAbc(groupConfig, history);
                        AbcGroupDownloadHistory.Save(groupHistoryFileName, groupDownloadHistory);

                        NotifyProgress($"Processing data for {groupConfig.Group}");
                        Task.Delay(10);
                        LoadDataFromFolder(ABC_TMP_FOLDER);

                        InstrumentDownloadHistory.Save(HistoryFile, InstrumentsHistory);
                    }
                }
                catch (AggregateException aex)
                {
                    MessageBox.Show(aex.InnerExceptions.First().Message, "Aggregate Download error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Download error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                    var destFile = f.Replace(ABC_TMP_FOLDER, ABC_PROCESSED_FOLDER);
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

                if (!StockDictionary.Instruments.TryGetValue(abcId, out var instrument))
                    continue;

                var history = GetDownloadHistory(instrument);
                if (history.LastDate == DateTime.MinValue)
                {
                    var dataSerie = instrument.GetDataSerie(DefaultDuration);

                    history.LastDate = dataSerie?.LastValue == null ? DateTime.MinValue : dataSerie.LastValue.DATE;
                }

                var newBars = serieData.Where(row => DateTime.Parse(row[1], frenchCulture) > history.LastDate).Select(row => new StockBar()
                {
                    open = float.Parse(row[2], CultureInfo.InvariantCulture),
                    high = float.Parse(row[3], CultureInfo.InvariantCulture),
                    low = float.Parse(row[4], CultureInfo.InvariantCulture),
                    close = float.Parse(row[5], CultureInfo.InvariantCulture),
                    volume = long.Parse(row[6], frenchCulture),
                    dateTicks = DateTime.Parse(row[1], frenchCulture).ToBinary()
                }).OrderBy(d => d.dateTicks).ToArray();

                if (newBars.Length > 0)
                {
                    StockBar.SerializeAppend(GetInstrumentFilePath(instrument), newBars);

                    history.LastDate = DateTime.FromBinary(newBars.Last().dateTicks);
                    history.DownloadDate = DateTime.Now;
                }
            }
        }
        public bool LoadFromCSV(StockSerie stockSerie)
        {
            return true;
        }

        List<AbcGroupDownloadHistory> groupDownloadHistory;

        string groupHistoryFileName;

        static readonly Dictionary<Market, TimeSpan> marketDownloadTimes = new Dictionary<Market, TimeSpan>
        {
            { Market.EURONEXT, new TimeSpan(18, 15, 0) },
            { Market.XETRA, new TimeSpan(21, 00, 0) },
            { Market.NYSE, new TimeSpan(6, 0, 0) }
        };

        private void DownloadGroupFromAbc(ABCGroup groupConfig, AbcGroupDownloadHistory history)
        {
            if (DateTime.Now > history.NextDownload)
            {
                var startDate = history.NextDownload == DateTime.MinValue ? new DateTime(ARCHIVE_START_YEAR, 1, 1) : history.LastDownload.Date.AddDays(-3);
                if (DownloadMonthlyFileFromABC(ABC_TMP_FOLDER, startDate, DateTime.Today, groupConfig, false))
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

                            if (history.NextDownload.DayOfWeek == DayOfWeek.Sunday)
                                history.NextDownload = history.NextDownload.AddDays(2);
                            if (history.NextDownload.DayOfWeek == DayOfWeek.Monday)
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

        private bool DownloadMonthlyFileFromABC(string destFolder, DateTime startDate, DateTime endDate, ABCGroup group, bool useCache = false)
        {
            bool success = true;

            StockLog.Write($"Downloading data for {group.Group} from {startDate.ToShortDateString()}");
            NotifyProgress($"Downloading data for {group.Group} from {startDate.ToShortDateString()}");
            try
            {
                if (startDate.Month != endDate.Month || startDate.Year != endDate.Year)
                {
                    DownloadMonthlyFileHistoryFromABC(destFolder, startDate, group, true);
                    startDate = new DateTime(endDate.Year, endDate.Month, 1);
                }

                string fileName = destFolder + @"\" + group.AbcGroup + "_" + endDate.Year + "_" + endDate.Month.ToString("0#") + ".csv";

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

        public DateTime GetLastDate(StockInstrument instrument)
        {
            var dataSerie = instrument.GetDataSerie(DefaultDuration);
            return dataSerie?.LastValue?.DATE == null ? DateTime.MinValue : dataSerie.LastValue.DATE;
        }

        public override bool RemoveEntry(StockInstrument instrument)
        {
            if (!string.IsNullOrEmpty(instrument.Isin) && !excludeList.Contains(instrument.Isin))
            {
                excludeList.Add(instrument.Isin);
                File.WriteAllLines(excludeFileName, excludeList);
            }
            return false;
        }

        public override void ForceDownloadData(StockInstrument instrument)
        {
            string filePattern = instrument.Isin + "_" + instrument.Symbol + "_" + instrument.Group.ToString() + "_*.csv";
            string fileName;
            StockLog.Write(instrument.DisplayName + " " + instrument.Isin);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                StockLog.Write("Network is Available");
                int nbFile = 0;
                var isin = instrument.Isin;
                if (instrument.Group == Groups.USA)
                    isin += "u";

                int year = DateTime.Today.Year;
                for (year = DateTime.Today.Year - 1; year >= ARCHIVE_START_YEAR; year--)
                {
                    fileName = filePattern.Replace("*", year.ToString());
                    if (!AbcClient.DownloadIsinYear(Path.Combine(ABC_TMP_FOLDER, fileName), year, isin))
                    {
                        Task.Delay(500).Wait();
                        break;
                    }
                    nbFile++;
                }
                year = DateTime.Today.Year;
                fileName = filePattern.Replace("*", year.ToString());
                if (AbcClient.DownloadIsin(Path.Combine(ABC_TMP_FOLDER, fileName), new DateTime(year, 1, 1), DateTime.Today, isin))
                {
                    nbFile++;
                }
                if (nbFile == 0)
                    return;

                // Parse loaded files
                List<StockBar> bars = new List<StockBar>();
                foreach (var csvFileName in Directory.GetFiles(ABC_TMP_FOLDER, filePattern).OrderBy(f => f))
                {
                    foreach (var line in File.ReadAllLines(csvFileName))
                    {
                        if (string.IsNullOrEmpty(line) || line.StartsWith("<") || line.StartsWith("/"))
                            break;
                        string[] row = line.Split(';');
                        bars.Add(new StockBar()
                        {
                            open = float.Parse(row[2], CultureInfo.InvariantCulture),
                            high = float.Parse(row[3], CultureInfo.InvariantCulture),
                            low = float.Parse(row[4], CultureInfo.InvariantCulture),
                            close = float.Parse(row[5], CultureInfo.InvariantCulture),
                            volume = long.Parse(row[6], frenchCulture),
                            dateTicks = DateTime.Parse(row[1], frenchCulture).ToBinary()
                        });
                    }
                    File.Delete(csvFileName);
                }

                StockBar.Serialize(GetInstrumentFilePath(instrument), bars.ToArray());
            }
            else
            {
                StockLog.Write("Network is not Available");
            }
        }

        public override DataSerie DownloadData(StockInstrument instrument)
        {
            StockLog.Write($"DownloadABCData Group:{instrument.Group} - {instrument.DisplayName}");

            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return null;

            DataSerie dataSerie = LoadData(instrument, DefaultDuration);
            if (dataSerie?.LastValue == null)
            {
                ForceDownloadData(instrument);
                return instrument.GetDataSerie(DefaultDuration);
            }

            NotifyProgress($"Downloading {instrument.DisplayName}");

            string fileName = Path.Combine(ABC_TMP_FOLDER, instrument.Id + ".csv");
            if (AbcClient.DownloadIsin(fileName, dataSerie.LastValue.DATE.AddDays(1), DateTime.Today, instrument.Isin))
            {
                var bars = this.LoadDataFromAbcFile(fileName);
                if (bars != null && bars.Count(b => b.DATE > dataSerie.LastValue.DATE) > 0)
                {
                    var newBars = dataSerie.Values.Union(bars.Where(b => b.DATE > dataSerie.LastValue.DATE)).ToArray();

                    StockBar.Serialize(GetInstrumentFilePath(instrument), newBars);
                    DataSerie newSerie = new DataSerie(instrument, DefaultDuration, newBars);

                    instrument.SetDataSerie(DefaultDuration, newSerie);

                    var history = GetDownloadHistory(instrument);
                    history.LastDate = RefSerie.LastValue.DATE;
                    history.DownloadDate = DateTime.Now;
                }
                else
                {
                    StockLog.Write($"No data for {instrument.DisplayName}");
                }

                File.Delete(fileName);
            }

            return dataSerie;
        }

        /// <summary>
        /// Parse file downloaded from ABC file and return StockDailyValue list. File format: ISIN,Date,Open,High,Low,Close,Volume
        /// ex: FR0000120404;02/01/12;19.735;20.03;19.45;19.94;418165
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private IEnumerable<StockDailyValue> LoadDataFromAbcFile(string fileName)
        {
            List<StockDailyValue> dailValues = null;
            if (File.Exists(fileName))
            {
                dailValues = new List<StockDailyValue>();
                using StreamReader sr = new StreamReader(fileName);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().Trim();
                    if (string.IsNullOrEmpty(line) || line.StartsWith("<") || line.StartsWith("/"))
                        continue;
                    string[] row = line.Split(';');
                    if (row.Length < 7)
                        return null;
                    var readValue = new StockDailyValue(
                        float.Parse(row[2], frenchCulture),
                        float.Parse(row[3], frenchCulture),
                        float.Parse(row[4], frenchCulture),
                        float.Parse(row[5], frenchCulture),
                        long.Parse(row[6], frenchCulture),
                        DateTime.Parse(row[1], frenchCulture));

                    if (readValue != null)
                        dailValues.Add(readValue);
                }
            }
            return dailValues;
        }


        /// <summary>
        /// Create stock series from label file. Download label files once a month id download is true. Only labels are downloaded no data.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="download"></param>
        /// <returns></returns>
        private bool InitAbcGroup(ABCGroup config, bool download)
        {
            var destFolder = config.LabelOnly ? ABC_DAILY_CFG_GROUP_FOLDER : ABC_DAILY_CFG_FOLDER;
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
            if (config.Group == Groups.SRD || config.Group == Groups.SRD_LO)
            {
                InitSRDFromLibelleFile(fileName, config.Group);
            }

            return true;
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

                    var id = row[0];
                    var isin = id.Substring(0, 12);
                    if (excludeList.Contains(isin))
                        continue;

                    if (!StockDictionary.Instruments.ContainsKey(id))
                    {
                        var existingInstrument = StockDictionary.Instruments.Values.FirstOrDefault(s => s.Isin == isin);
                        if (existingInstrument != null)
                        {
                            StockLog.Write($"Duplicate ISIN {isin}:{config.Group}:{stockName} already listed from {existingInstrument.Group}:{existingInstrument.DisplayName}");
                            continue;
                        }

                        var abcSuffix = id.Length > 12 ? id[12] : 'p';
                        var instrument = new StockInstrument()
                        {
                            StockSerie = null,

                            Id = id,
                            Name = stockName,
                            Isin = isin,
                            Provider = DataProvider.ABC,
                            Symbol = row[2],
                            AbcSuffix = abcSuffix,
                            Group = config.Group,
                            Market = config.Market
                        };
                        StockDictionary.Instruments.Add(instrument.Id, instrument);
                    }
                    else
                    {
                        StockLog.Write("Duplicate " + config.Group + ";" + line + " already in group " + StockDictionary.Instruments[id].Group);
                    }
                }
            }
        }
        private void InitSRDFromLibelleFile(string fileName, Groups group)
        {
            //if (File.Exists(fileName))
            //{
            //    using StreamReader sr = new StreamReader(fileName, true);
            //    string line;
            //    sr.ReadLine(); // Skip first line
            //    while (!sr.EndOfStream)
            //    {
            //        line = sr.ReadLine();
            //        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
            //        {
            //            string[] row = line.Split(';');
            //            string stockName = row[1].ToUpper().Replace(" - ", " ").Replace("-", " ").Replace("  ", " ");
            //            if (StockDictionary.Instruments.ContainsKey(stockName))
            //            {
            //                if (group == Groups.SRD)
            //                {
            //                    StockDictionary.Instruments[stockName].SRD = true;
            //                }
            //                if (group == Groups.SRD_LO)
            //                {
            //                    StockDictionary.Instruments[stockName].SRD_LO = true;
            //                }
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    StockLog.Write("File does not exist");
            //}
        }

        private bool IsinMatchGroup(ABCGroup group, string line)
        {
            if (group.Prefixes != null && group.Prefixes.Length > 0)
            {
                return group.Prefixes.Any(prefix => line.StartsWith(prefix));
            }
            return true;
        }

        static SortedDictionary<Groups, List<string>> groupSeries = new SortedDictionary<Groups, List<string>>();
        public bool BelongsToGroup(StockInstrument instrument, Groups group)
        {
            if (group == instrument.Group || group == Groups.ALL_STOCKS)
                return true;

            switch (group)
            {
                case Groups.EURO_A_B:
                    return instrument.Group == Groups.EURO_A || instrument.Group == Groups.EURO_B;
                case Groups.EURO_A_B_C:
                    return instrument.Group == Groups.EURO_A || instrument.Group == Groups.EURO_B || instrument.Group == Groups.EURO_C;
                case Groups.CACALL:
                    return instrument.Group == Groups.EURO_A || instrument.Group == Groups.EURO_B || instrument.Group == Groups.EURO_C || instrument.Group == Groups.ALTERNEXT;
                case Groups.PEA_EURONEXT:
                    return instrument.Group == Groups.EURO_A || instrument.Group == Groups.EURO_B || instrument.Group == Groups.EURO_C || instrument.Group == Groups.ALTERNEXT
                        || instrument.Group == Groups.BELGIUM || instrument.Group == Groups.HOLLAND || instrument.Group == Groups.PORTUGAL;
                case Groups.PEA:
                    return instrument.Group == Groups.EURO_A || instrument.Group == Groups.EURO_B || instrument.Group == Groups.EURO_C || instrument.Group == Groups.ALTERNEXT
                        || instrument.Group == Groups.BELGIUM || instrument.Group == Groups.HOLLAND || instrument.Group == Groups.PORTUGAL
                        || instrument.Group == Groups.ITALIA || instrument.Group == Groups.GERMANY || instrument.Group == Groups.SPAIN;
            }

            if (!groupSeries.ContainsKey(group))
                return false;

            var groupList = groupSeries[group];
            if (groupList == null)
            {
                groupList = new List<string>();
                var abcGroup = abcGroupConfig.FirstOrDefault(g => g.Group == group);
                // parse group definition
                string fileName = Path.Combine(ABC_DAILY_CFG_GROUP_FOLDER, $"{abcGroup.AbcGroup}.csv");
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
                            var serie = StockDictionary.Instruments.Values.FirstOrDefault(s => s.Isin == row[0]);
                            if (serie != null)
                            {
                                groupList.Add(serie.DisplayName);
                            }
                        }
                    }
                    groupSeries[group] = groupList;
                }
                else
                {
                    MessageBox.Show($"Group definition file not found for Group: {group}", "ABD DataProvider Group error");
                }
            }


            return groupList != null && groupList.Contains(instrument.Id);
        }

        TimeSpan closeTime = new TimeSpan(17, 35, 0);
        TimeSpan openTime = new TimeSpan(09, 0, 0);
        TimeSpan delay = new TimeSpan(0, 0, 5);

        public override bool NeedDownload(StockInstrument instrument)
        {
            var history = InstrumentsHistory.FirstOrDefault(h => h.Id == instrument.Id);
            if (history.LastDate == DateTime.MinValue)
                return true;

            var now = DateTime.Now;
            var isLate = now.TimeOfDay > closeTime;

            if (now.DayOfWeek == DayOfWeek.Saturday || // Check if week-end
                now.DayOfWeek == DayOfWeek.Sunday ||
                (now.DayOfWeek == DayOfWeek.Monday && !isLate))
            {
                if ((now.Date - history.LastDate.Date).TotalDays > 3)
                    return true;

                return history.LastDate.DayOfWeek != DayOfWeek.Friday;
            }
            else if (isLate)
            {
                return history.LastDate != now.Date;
            }

            return (now.Date - history.LastDate.Date).TotalDays > 1;
        }

        protected override bool UpdateIntradayDataSpecific(StockInstrument instrument)
        {
            var dailyValue = AbcClient.DownloadDailyValue(instrument.Symbol + instrument.AbcSuffix);
            if (dailyValue == null)
                return false;

            instrument.ClearCache();
            var dataSerie = instrument.GetDefaultDataSerie();
            if (dataSerie == null)
                return false;

            dataSerie.AddBar(dailyValue);

            return true;
        }

    }
}
