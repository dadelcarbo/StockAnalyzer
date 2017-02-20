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
using System.Threading;
using StockAnalyzer.StockWeb;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class ABCDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private string ABC_INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\ABC";
        static private string ABC_DAILY_FOLDER = DAILY_SUBFOLDER + @"\ABC";
        static private string ABC_DAILY_CFG_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl";
        static private string ABC_DAILY_CFG_GROUP_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\group";
        static private string ABC_DAILY_CFG_SECTOR_FOLDER = DAILY_SUBFOLDER + @"\ABC\lbl\sector";
        private static string FINANCIAL_SUBFOLDER = @"\data\financial";
        private static string AGENDA_SUBFOLDER = @"\data\agenda";
        static private string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\ABC";
        static private string CONFIG_FILE = @"\EuronextDownload.cfg";
        static private string CONFIG_FILE_USER = @"\EuronextDownload.user.cfg";

        static private string labelViewState = string.Empty;
        static private string labelEventValidation = string.Empty;
        static private string intradayViewState = string.Empty;
        static private string intradayEventValidation = string.Empty;
        static private string dailyViewState = string.Empty;
        static private string dailyViewStateGenerator = string.Empty;
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
            stockDictionary = dictionary; // Save dictionary for future use in daily download

            // Create data folder if not existing
            if (!Directory.Exists(rootFolder + FINANCIAL_SUBFOLDER))
            {
                Directory.CreateDirectory(rootFolder + FINANCIAL_SUBFOLDER);
            }
            if (!Directory.Exists(rootFolder + AGENDA_SUBFOLDER))
            {
                Directory.CreateDirectory(rootFolder + AGENDA_SUBFOLDER);
            }
            if (!Directory.Exists(rootFolder + ABC_DAILY_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ABC_DAILY_FOLDER);
            }
            if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(rootFolder + ABC_INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ABC_INTRADAY_FOLDER);
            }
            if (!Directory.Exists(rootFolder + ABC_DAILY_CFG_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ABC_DAILY_CFG_FOLDER);
            }
            if (!Directory.Exists(rootFolder + ABC_DAILY_CFG_SECTOR_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ABC_DAILY_CFG_SECTOR_FOLDER);
            }
            if (!Directory.Exists(rootFolder + ABC_DAILY_CFG_GROUP_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ABC_DAILY_CFG_GROUP_FOLDER);
            }
            else
            {
                foreach (string file in Directory.GetFiles(rootFolder + ABC_INTRADAY_FOLDER))
                {
                    // Purge intraday files at each start
                    File.Delete(file);
                }
            }

            //// Parse SBF120.txt file
            string fileName = rootFolder + CONFIG_FILE;
            InitFromFile(rootFolder, download, fileName);
            fileName = rootFolder + CONFIG_FILE_USER;
            InitFromFile(rootFolder, download, fileName);

            // Init From LBL file
            //            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_GROUP_FOLDER, new string[] { "srdp", "srdlop" }, StockSerie.Groups.SRD);
            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_GROUP_FOLDER, "srdp", StockSerie.Groups.SRD);
            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_GROUP_FOLDER, "srdlop", StockSerie.Groups.SRD_LO);
            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_FOLDER, "eurolistAp", StockSerie.Groups.EURO_A);
            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_FOLDER, "eurolistBp", StockSerie.Groups.EURO_B);
            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_FOLDER, "eurolistCp", StockSerie.Groups.EURO_C);
            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_FOLDER, "alterp", StockSerie.Groups.ALTERNEXT);
            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_FOLDER, "indicessecp", StockSerie.Groups.SECTORS_CAC);
            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_GROUP_FOLDER, "xcac40p", StockSerie.Groups.CAC40);
            DownloadLibelleFromABC(rootFolder + ABC_DAILY_CFG_FOLDER, "sp500u", StockSerie.Groups.SP500);

            //DownloadMonthlyFileFromABC(rootFolder + ABC_DAILY_FOLDER, DateTime.Today, "eurolistap");

            // Init from Libelles
            foreach (string file in Directory.GetFiles(rootFolder + ABC_DAILY_CFG_FOLDER))
            {
                InitFromLibelleFile(rootFolder, download, file);
            }

            // Download sector libelle
            foreach (string sectorID in SectorCodes.Keys)
            {
                if (DownloadSectorFromABC(rootFolder + ABC_DAILY_CFG_SECTOR_FOLDER, sectorID))
                {
                    AssignSector(rootFolder + ABC_DAILY_CFG_SECTOR_FOLDER, sectorID);
                }
                else
                {
                    break;
                }
            }
        }

        private void AssignSector(string destFolder, string sectorID)
        {
            string fileName = destFolder + @"\" + sectorID + ".txt";
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
                            StockSerie stockSerie = stockDictionary.Values.FirstOrDefault(s => s.ISIN == row[0].ToUpper());
                            if (stockSerie == null)
                            {
                                StockLog.Write(row[0].ToUpper() + " " + row[1].ToUpper() + " Not found!!!");
                            }
                            else
                            {
                                stockSerie.SectorID = sectorID;
                            }
                        }
                    }
                }
            }
        }

        private void InitFromLibelleFile(string rootFolder, bool download, string fileName)
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
                        if (!line.StartsWith("#"))
                        {
                            string[] row = line.Split(';');
                            if (!stockDictionary.ContainsKey(row[1].ToUpper()))
                            {
                                StockSerie stockSerie = new StockSerie(row[1].ToUpper(), row[2], row[0], group, StockDataProvider.ABC);

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

        private void InitFromFile(string rootFolder, bool download, string fileName)
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
                        if (!line.StartsWith("#"))
                        {
                            string[] row = line.Split(';');
                            StockSerie stockSerie = new StockSerie(row[1], row[3], row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[4]), StockDataProvider.ABC);
                            if (!stockDictionary.ContainsKey(row[1]))
                            {
                                stockDictionary.Add(row[1], stockSerie);
                                //if (stockSerie.StockGroup == StockSerie.Groups.CAC40)
                                //{
                                //   StockSerie stockSerieRS = new StockSerie(row[1] + "_RS", row[3] + "_RS", StockSerie.Groups.CAC40_RS, StockDataProvider.ABC);
                                //   stockDictionary.Add(stockSerieRS.StockName, stockSerieRS);
                                //}
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

        static string loadingGroup = null;
        static List<string> loadedGroups = new List<string>();
        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            StockLog.Write("Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName + " - " + stockSerie.Count);
            bool res = false;

            //if (stockSerie.StockGroup == StockSerie.Groups.CAC40_RS)
            //{
            //   StockSerie baseSerie = stockDictionary[stockSerie.StockName.Replace("_RS", "")];
            //   StockSerie cacSerie = stockDictionary["CAC40"];

            //   return stockSerie.GenerateRelativeStrenthStockSerie(baseSerie, cacSerie);
            //}
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
                    abcGroup = "alterp";
                    break;
                case StockSerie.Groups.SECTORS_CAC:
                    abcGroup = "indicessecp";
                    break;
                default:
                    break;
            }
            string fileName = null;
            string[] files;
            if (abcGroup != null)
            {
                try
                {
                    if (!loadedGroups.Contains(abcGroup))
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
                        var groupFiles =
                           System.IO.Directory.GetFiles(rootFolder + ARCHIVE_FOLDER, fileName).OrderByDescending(s => s);
                        foreach (string archiveFileName in groupFiles)
                        {
                            NotifyProgress("Loading data for " + Path.GetFileNameWithoutExtension(archiveFileName));
                            if (!ParseABCGroupCSVFile(archiveFileName, stockSerie.StockGroup)) break;
                            else
                            {
                                res = true;
                            }
                        }
                        groupFiles =
                           System.IO.Directory.GetFiles(rootFolder + ABC_DAILY_FOLDER, fileName).OrderByDescending(s => s);
                        foreach (string currentFileName in groupFiles)
                        {
                            res = ParseABCGroupCSVFile(currentFileName, stockSerie.StockGroup);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    StockLog.Write(ex);
                }
                finally
                {
                    loadingGroup = null;
                    loadedGroups.Add(abcGroup);
                }
                // @@@@ stockSerie.ClearBarDurationCache(); Removed as I don't know why it's here.
            }

            if (stockSerie.Count == 0)
            {
                // Read archive first
                fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() +
                           "_*.csv";
                files = System.IO.Directory.GetFiles(rootFolder + ARCHIVE_FOLDER, fileName);
                foreach (string archiveFileName in files)
                {
                    res |= ParseCSVFile(stockSerie, archiveFileName);
                }

                // Read daily value
                fileName = rootFolder + ABC_DAILY_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" +
                           stockSerie.StockGroup.ToString() + ".csv";
                res |= ParseCSVFile(stockSerie, fileName);
            }
            // Read intraday
            if (res && stockSerie.Keys.Last() != DateTime.Today)
            {
                if (stockSerie.BelongsToGroup(StockSerie.Groups.SRD))
                {
                    fileName = rootFolder + ABC_INTRADAY_FOLDER + "\\" + DateTime.Today.ToString("yyMMdd_") + "SRD.csv";
                    ParseABCGroupCSVFile(fileName, StockSerie.Groups.SRD, true);
                    return true;
                }
                else if (stockSerie.BelongsToGroup(StockSerie.Groups.SP500))
                {
                    fileName = rootFolder + ABC_INTRADAY_FOLDER + "\\" + DateTime.Today.ToString("yyMMdd_") + "SP500.csv";
                    ParseABCGroupCSVFile(fileName, StockSerie.Groups.SP500, true);
                    return true;
                }
                else if (stockSerie.BelongsToGroup(StockSerie.Groups.INDICES))
                {
                    fileName = rootFolder + ABC_INTRADAY_FOLDER + "\\" + DateTime.Today.ToString("yyMMdd_") + "IndicesFR.csv";
                    ParseABCIntradayFile(stockSerie, fileName);
                    return true;
                }
            }
            return res;
        }

        private bool ParseABCGroupCSVFile(string fileName, StockSerie.Groups group, bool intraday = false)
        {
            //StockLog.Write(fileName);

            if (!File.Exists(fileName)) return false;
            StockSerie stockSerie = null;
            using (StreamReader sr = new StreamReader(fileName, true))
            {
                string line = sr.ReadLine();
                string previousISIN = string.Empty;
                while (!sr.EndOfStream)
                {
                    string[] row = line.Split(';');
                    if (previousISIN != row[0])
                    {
                        stockSerie = stockDictionary.Values.FirstOrDefault(s => s.ISIN == row[0] && s.StockGroup == group);
                        previousISIN = row[0];
                    }
                    if (stockSerie != null)
                    {
                        DateTime date;
                        if (intraday)
                        {
                            date = File.GetLastWriteTime(fileName);
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
                                  stockSerie.StockName,
                                  float.Parse(row[2], frenchCulture),
                                  float.Parse(row[3], frenchCulture),
                                  float.Parse(row[4], frenchCulture),
                                  float.Parse(row[5], frenchCulture),
                                  long.Parse(row[6], frenchCulture),
                                  date);
                                stockSerie.Add(dailyValue.DATE, dailyValue);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    line = sr.ReadLine();
                }
            }
            return true;
        }

        private bool ParseABCIntradayFile(StockSerie stockSerie, string fileName)
        {
            StockLog.Write("ParseABCIntradayFile: " + fileName);

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
                            float.Parse(row[2], usCulture),
                            float.Parse(row[3], usCulture),
                            float.Parse(row[4], usCulture),
                            float.Parse(row[5], usCulture),
                            long.Parse(row[6]),
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
            StockLog.Write("DownloadDailyData Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                bool isUpTodate = false;
                stockSerie.Initialise();
                if (stockSerie.Count > 0)
                {
                    DateTime lastDate = stockSerie.Keys.Last();
                    if (lastDate.TimeOfDay != TimeSpan.Zero)
                    {
                        stockSerie.Remove(lastDate);
                        lastDate = stockSerie.Keys.Last();
                    }

                    isUpTodate = (lastDate >= DateTime.Today) ||
                                 (lastDate.DayOfWeek == DayOfWeek.Friday && (DateTime.Now - lastDate).Days <= 3 &&
                                  (DateTime.Today.DayOfWeek == DayOfWeek.Monday && DateTime.Now.Hour < 18)) ||
                                 (lastDate == DateTime.Today.AddDays(-1) && DateTime.Now.Hour < 18);

                    if (!isUpTodate)
                    {
                        NotifyProgress("Downloading " + stockSerie.StockGroup.ToString() + " - " + stockSerie.StockName);

                        // Happy new year !!! it's time to archive old data...
                        for (int year = lastDate.Year; year < DateTime.Today.Year; year++)
                        {
                            if (
                               !File.Exists(rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName + "_" +
                                            stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" +
                                            year.ToString() + ".csv"))
                            {
                                this.DownloadFileFromProvider(rootFolder + ARCHIVE_FOLDER,
                                   stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() +
                                   "_" + year.ToString() + ".csv", new DateTime(year, 1, 1), new DateTime(year, 12, 31),
                                   stockSerie.ShortName);
                            }
                        }
                        DateTime startDate = new DateTime(DateTime.Today.Year, 01, 01);
                        string fileName = stockSerie.ShortName + "_" + stockSerie.StockName + "_" +
                                          stockSerie.StockGroup.ToString() + ".csv";
                        this.DownloadFileFromProvider(rootFolder + ABC_DAILY_FOLDER, fileName, startDate, DateTime.Today,
                           stockSerie.ISIN);

                        if (stockSerie.StockName == "CAC40")
                        // Check if something new has been downloaded using CAC40 as the reference for all downloads
                        {
                            this.ParseCSVFile(stockSerie, rootFolder + ABC_DAILY_FOLDER + "\\" + fileName);
                            if (lastDate == stockSerie.Keys.Last())
                            {
                                this.needDownload = false;
                            }
                            else
                            {
                                DownloadMonthlyFileFromABC(rootFolder + ABC_DAILY_FOLDER, DateTime.Today, "eurolistap");
                                DownloadMonthlyFileFromABC(rootFolder + ABC_DAILY_FOLDER, DateTime.Today, "eurolistbp");
                                DownloadMonthlyFileFromABC(rootFolder + ABC_DAILY_FOLDER, DateTime.Today, "eurolistcp");
                                DownloadMonthlyFileFromABC(rootFolder + ABC_DAILY_FOLDER, DateTime.Today, "alterp");
                                //DownloadMonthlyFileFromABC(rootFolder + ABC_DAILY_FOLDER, DateTime.Today, "srdp");
                                DownloadMonthlyFileFromABC(rootFolder + ABC_DAILY_FOLDER, DateTime.Today, "sp500u");
                                DownloadMonthlyFileFromABC(rootFolder + ABC_DAILY_FOLDER, DateTime.Today, "indicessecp");
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
                    stockSerie.IsInitialised = isUpTodate; // && !needReloadIntraday; Why need reload intraday ???
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
                            DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "eurolistap");
                            DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "eurolistbp");
                            DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "eurolistcp");
                            DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "alterp");
                            //DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "srdp");
                            DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "sp500u");
                            DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "indicessecp");
                        }
                    }
                    for (int i = lastDate.Year - 1; i > ARCHIVE_START_YEAR; i--)
                    {
                        if (
                           !this.DownloadFileFromProvider(rootFolder + ARCHIVE_FOLDER,
                              stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + "_" +
                              i.ToString() + ".csv", new DateTime(i, 1, 1), new DateTime(i, 12, 31), stockSerie.ISIN))
                        {
                            break;
                        }
                        if (stockSerie.StockName == "CAC40")
                        {
                            for (int m = 12; m >= 1; m--)
                            {
                                DateTime month = new DateTime(i, m, 1);
                                DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "eurolistap");
                                DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "eurolistbp");
                                DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "eurolistcp");
                                DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "alterp");
                                //DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "srdp");
                                DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "sp500u");
                                DownloadMonthlyFileFromABC(rootFolder + ARCHIVE_FOLDER, month, "indicessecp");
                            }
                        }
                    }
                    this.DownloadFileFromProvider(rootFolder + ABC_DAILY_FOLDER,
                       stockSerie.ShortName + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv",
                       lastDate, DateTime.Today, stockSerie.ISIN);
                }
            }
            return true;
        }
        public override bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
        {
            StockLog.Write("DownloadIntradayData Group: " + stockSerie.StockGroup + " - " + stockSerie.StockName);

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
                if (stockSerie.BelongsToGroup(StockSerie.Groups.SRD))
                {
                    fileName = DateTime.Today.ToString("yyMMdd_") + "SRD.csv";
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
                    foreach (StockSerie serie in stockDictionary.Values.Where(s => s.BelongsToGroup(stockSerie.StockGroup)))
                    {
                        serie.IsInitialised = false;
                    }
                    loadedGroups.Clear();
                    //foreach (StockSerie serie in stockDictionary.Values.Where(s => s.DataProvider == StockDataProvider.Breadth && s.StockName.Contains("SBF120") || s.StockName.Contains("CAC40")))
                    //{
                    //   serie.IsInitialised = false;
                    //}
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
        private bool DownloadLibelleFromABC(string destFolder, string[] groupNames, StockSerie.Groups group)
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
                    // Send POST request
                    string url = "https://www.abcbourse.com/download/libelles.aspx";
                    if (dailyViewState == string.Empty)
                    {
                        // Get ViewState 
                        using (WebClient webClient = new WebClient())
                        {
                            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36");
                            byte[] response = webClient.DownloadData(url);

                            string htmlContent = Encoding.ASCII.GetString(response);
                            dailyViewState = ExtractValue(htmlContent, "__VIEWSTATE");
                            dailyViewStateGenerator = ExtractValue(htmlContent, "__VIEWSTATEGENERATOR");
                            dailyEventValidation = ExtractValue(htmlContent, "__EVENTVALIDATION");
                        }
                    }

                    string postData = "ctl00_BodyABC_ToolkitScriptManager1_HiddenField=%3B%3BAjaxControlToolkit%2C+Version%3D3.0.20229.20843%2C+Culture%3Dneutral%2C+PublicKeyToken%3D28f01b0e84b6d53e%3Afr-FR%3A3b7d1b28-161f-426a-ab77-b345f2c428f5%3A865923e8%3A9b7907bc%3A411fea1c%3Ae7c87f07%3A91bd373d%3Abbfda34c%3A30a78ec5%3A9349f837%3Ad4245214%3A77c58d20%3A14b56adc%3A8e72a662%3Aacd642d2%3A596d588c%3A269a19ae&"
                        + "__EVENTTARGET=&"
                        + "__EVENTARGUMENT=&"
                        + "__VIEWSTATE=" + dailyViewState + "&"
                        + "__VIEWSTATEGENERATOR=" + dailyViewStateGenerator + "&"
                        + "__EVENTVALIDATION=" + dailyEventValidation + "&";

                    postData = "__VIEWSTATE=%2FwEPDwULLTEzOTkwMTQxNjkPZBYCZg9kFgICBA9kFgYCCQ9kFgICVQ9kFgJmDxYCHgdWaXNpYmxlZ2QCDQ9kFgJmDxYCHwBnZAIPD2QWAgIBDw8WAh4EVGV4dAUpQmFzY3VsZXIgc3VyIGxhIHZlcnNpb24gY2xhc3NpcXVlIGR1IHNpdGVkZBgBBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WKQUVY3RsMDAkQm9keUFCQyR4Y2FjNDBwBRZjdGwwMCRCb2R5QUJDJHhzYmYxMjBwBRVjdGwwMCRCb2R5QUJDJHhjYWNhdHAFFmN0bDAwJEJvZHlBQkMkeGNhY24yMHAFGGN0bDAwJEJvZHlBQkMkeGNhY3NtYWxscAUVY3RsMDAkQm9keUFCQyR4Y2FjNjBwBRZjdGwwMCRCb2R5QUJDJHhjYWNsNjBwBRVjdGwwMCRCb2R5QUJDJHhjYWNtc3AFFWN0bDAwJEJvZHlBQkMkeGJlbDIwZwUVY3RsMDAkQm9keUFCQyR4YWV4MjVuBRFjdGwwMCRCb2R5QUJDJGRqdQUSY3RsMDAkQm9keUFCQyRuYXN1BRRjdGwwMCRCb2R5QUJDJHNwNTAwdQUWY3RsMDAkQm9keUFCQyRnZXJtYW55ZgURY3RsMDAkQm9keUFCQyR1a2UFEmN0bDAwJEJvZHlBQkMkYmVsZwUSY3RsMDAkQm9keUFCQyRkZXZwBRRjdGwwMCRCb2R5QUJDJHNwYWlubQUVY3RsMDAkQm9keUFCQyRpdGFsaWFpBRNjdGwwMCRCb2R5QUJDJGhvbGxuBRVjdGwwMCRCb2R5QUJDJGxpc2JvYWwFFGN0bDAwJEJvZHlBQkMkc3dpdHpzBRJjdGwwMCRCb2R5QUJDJHVzYXUFFGN0bDAwJEJvZHlBQkMkYWx0ZXJwBRFjdGwwMCRCb2R5QUJDJGJzcAUYY3RsMDAkQm9keUFCQyRldXJvbGlzdEFwBRhjdGwwMCRCb2R5QUJDJGV1cm9saXN0QnAFGGN0bDAwJEJvZHlBQkMkZXVyb2xpc3RDcAUZY3RsMDAkQm9keUFCQyRldXJvbGlzdHplcAUaY3RsMDAkQm9keUFCQyRldXJvbGlzdGh6ZXAFGGN0bDAwJEJvZHlBQkMkaW5kaWNlc21rcAUZY3RsMDAkQm9keUFCQyRpbmRpY2Vzc2VjcAURY3RsMDAkQm9keUFCQyRtbHAFE2N0bDAwJEJvZHlBQkMkb2JsMnAFEmN0bDAwJEJvZHlBQkMkb2JscAUXY3RsMDAkQm9keUFCQyRvcGN2bTM2MHAFEmN0bDAwJEJvZHlBQkMkc3JkcAUUY3RsMDAkQm9keUFCQyRzcmRsb3AFFGN0bDAwJEJvZHlBQkMkdHJhY2twBRZjdGwwMCRCb2R5QUJDJHdhcnJhbnRzBRVjdGwwMCRCb2R5QUJDJGNiUGxhY2XV7VAmF5gdwy7DIiJT1Q8P3geCLQ%3D%3D&__VIEWSTATEGENERATOR=63AB8707&__EVENTVALIDATION=%2FwEdAC2uMiOe5%2Bi%2BKBXO4l4QxQh92AGy%2BFRpYOz7XDkkbfjubp9UXI7RwI%2BukRHnd%2BAlDZ7yE4oeSMQcqAToKX4%2BVY%2FoKwHPZ3LL3fdWqV0S%2FvWmetYHl%2BXtIMfr4sJ5HoKPeEGaXWKkENsUVjCs33ftb%2Bk6Vh68XGlO5A7hLzsl2zmozVHKtnVHMqNjuSl%2FVTLUSxGOrSXMajdQMItHxDOD4gI5oZA%2FrQy55rsm3Yy%2BuTl0%2FnRrfHed0TzZAp%2F%2By2dFmxusO8axFlSjvdrqSAJF9oAESNvpV6G124LKs01uIQT%2BzPLtwgDb4ZnV8AzgWlnJDQlBhudEBAhKHZIsMbDqQKObxt6eBSEoHlSQ0h6eQsjG3FU1EY8C1%2F0GGZDF0VWO2oYTcspg%2FQJIEo7yz4CR037atiIVgEYIzEhkb9KlYZmye7%2FnJk8Wqab3FraRKRA4NH4SMkbr0ssfhDNp48vrt1aToLK%2FxuYkxeDUJ2jyEChuCcZNAzb5On4rHEP9HywghYEYPSSOqNVqe2KgYnr%2BNJRx9At%2BumCzYS2uPoXq%2FdVJByyB7RsbMXUq2lzc8dt95PO%2BkCFfVa1MgDkVN3T3jY%2FKQIBPK12FXYyMGO0%2F5KPgqmM77ItwinSXDlQBQcTHG5JyTAMHmjunt2bP%2Fj%2FgPFOe%2F%2Baf%2FiQDbqNtvcnRfkH2ohjdh6dUQ5m%2B2VTNYWkGxattRg9EKKE5vey1FPRlCqZyDgvrv9lvrlshrlzdNlGwotAGLfLMcs2vyw1rETiNehQSuxs4fVNVenj9bBwNMtd92XoZ0fgAFx0VYRpgUubx6SaFj1P1ns1k7saP20CtOv40Vh1fIBS7G3r7SNtlzk3C0A%2F4Rmw2%2BXo6xnkHUnscHL5GCzjGbRjxavMDrnvi92ihmau6VuJALuZvH2%2BXItM49krxVbbQbx%2BrvmYdNFMYrxrqfXKoG%2FeBbQYV684ncHXIr7RWAGAEkKq%2FpI3%2Bjw0KrRp1bA%3D%3D&ctl00%24txtAutoComplete=&";

                    foreach (string groupName in groupNames)
                    {
                        postData += "ctl00%24BodyABC%24" + groupName + "=on&";
                    }

                    postData += "ctl00%24BodyABC%24Button1=T%C3%A9l%C3%A9charger";

                    // ctl00%24BodyABC%24srdp=on&ctl00%24BodyABC%24srdlop=on&ctl00%24BodyABC%24Button1=T%C3%A9l%C3%A9charger";

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

                    success = SaveResponseToFile(fileName, req);
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
                    // Send POST request
                    string url = "https://www.abcbourse.com/download/libelles.aspx";
                    if (dailyViewState == string.Empty)
                    {
                        // Get ViewState 
                        using (WebClient webClient = new WebClient())
                        {
                            webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36");
                            byte[] response = webClient.DownloadData(url);

                            string htmlContent = Encoding.ASCII.GetString(response);
                            dailyViewState = ExtractValue(htmlContent, "__VIEWSTATE");
                            dailyViewStateGenerator = ExtractValue(htmlContent, "__VIEWSTATEGENERATOR");
                            dailyEventValidation = ExtractValue(htmlContent, "__EVENTVALIDATION");
                        }
                    }

                    string postData = "ctl00_BodyABC_ToolkitScriptManager1_HiddenField=%3B%3BAjaxControlToolkit%2C+Version%3D3.0.20229.20843%2C+Culture%3Dneutral%2C+PublicKeyToken%3D28f01b0e84b6d53e%3Afr-FR%3A3b7d1b28-161f-426a-ab77-b345f2c428f5%3A865923e8%3A9b7907bc%3A411fea1c%3Ae7c87f07%3A91bd373d%3Abbfda34c%3A30a78ec5%3A9349f837%3Ad4245214%3A77c58d20%3A14b56adc%3A8e72a662%3Aacd642d2%3A596d588c%3A269a19ae&"
                        + "__EVENTTARGET=&"
                        + "__EVENTARGUMENT=&"
                        + "__VIEWSTATE=" + dailyViewState + "&"
                        + "__VIEWSTATEGENERATOR=" + dailyViewStateGenerator + "&"
                        + "__EVENTVALIDATION=" + dailyEventValidation + "&"
                        + "ctl00%24BodyABC%24$GROUPNAME=on&"
                        + "ctl00%24BodyABC%24Button1=T%C3%A9l%C3%A9charger";

                    postData = "__VIEWSTATE=%2FwEPDwULLTEzOTkwMTQxNjkPZBYCZg9kFgICBA9kFgYCCQ9kFgICVQ9kFgJmDxYCHgdWaXNpYmxlZ2QCDQ9kFgJmDxYCHwBnZAIPD2QWAgIBDw8WAh4EVGV4dAUpQmFzY3VsZXIgc3VyIGxhIHZlcnNpb24gY2xhc3NpcXVlIGR1IHNpdGVkZBgBBR5fX0NvbnRyb2xzUmVxdWlyZVBvc3RCYWNrS2V5X18WKQUVY3RsMDAkQm9keUFCQyR4Y2FjNDBwBRZjdGwwMCRCb2R5QUJDJHhzYmYxMjBwBRVjdGwwMCRCb2R5QUJDJHhjYWNhdHAFFmN0bDAwJEJvZHlBQkMkeGNhY24yMHAFGGN0bDAwJEJvZHlBQkMkeGNhY3NtYWxscAUVY3RsMDAkQm9keUFCQyR4Y2FjNjBwBRZjdGwwMCRCb2R5QUJDJHhjYWNsNjBwBRVjdGwwMCRCb2R5QUJDJHhjYWNtc3AFFWN0bDAwJEJvZHlBQkMkeGJlbDIwZwUVY3RsMDAkQm9keUFCQyR4YWV4MjVuBRFjdGwwMCRCb2R5QUJDJGRqdQUSY3RsMDAkQm9keUFCQyRuYXN1BRRjdGwwMCRCb2R5QUJDJHNwNTAwdQUWY3RsMDAkQm9keUFCQyRnZXJtYW55ZgURY3RsMDAkQm9keUFCQyR1a2UFEmN0bDAwJEJvZHlBQkMkYmVsZwUSY3RsMDAkQm9keUFCQyRkZXZwBRRjdGwwMCRCb2R5QUJDJHNwYWlubQUVY3RsMDAkQm9keUFCQyRpdGFsaWFpBRNjdGwwMCRCb2R5QUJDJGhvbGxuBRVjdGwwMCRCb2R5QUJDJGxpc2JvYWwFFGN0bDAwJEJvZHlBQkMkc3dpdHpzBRJjdGwwMCRCb2R5QUJDJHVzYXUFFGN0bDAwJEJvZHlBQkMkYWx0ZXJwBRFjdGwwMCRCb2R5QUJDJGJzcAUYY3RsMDAkQm9keUFCQyRldXJvbGlzdEFwBRhjdGwwMCRCb2R5QUJDJGV1cm9saXN0QnAFGGN0bDAwJEJvZHlBQkMkZXVyb2xpc3RDcAUZY3RsMDAkQm9keUFCQyRldXJvbGlzdHplcAUaY3RsMDAkQm9keUFCQyRldXJvbGlzdGh6ZXAFGGN0bDAwJEJvZHlBQkMkaW5kaWNlc21rcAUZY3RsMDAkQm9keUFCQyRpbmRpY2Vzc2VjcAURY3RsMDAkQm9keUFCQyRtbHAFE2N0bDAwJEJvZHlBQkMkb2JsMnAFEmN0bDAwJEJvZHlBQkMkb2JscAUXY3RsMDAkQm9keUFCQyRvcGN2bTM2MHAFEmN0bDAwJEJvZHlBQkMkc3JkcAUUY3RsMDAkQm9keUFCQyRzcmRsb3AFFGN0bDAwJEJvZHlBQkMkdHJhY2twBRZjdGwwMCRCb2R5QUJDJHdhcnJhbnRzBRVjdGwwMCRCb2R5QUJDJGNiUGxhY2XV7VAmF5gdwy7DIiJT1Q8P3geCLQ%3D%3D&__VIEWSTATEGENERATOR=63AB8707&__EVENTVALIDATION=%2FwEdAC2uMiOe5%2Bi%2BKBXO4l4QxQh92AGy%2BFRpYOz7XDkkbfjubp9UXI7RwI%2BukRHnd%2BAlDZ7yE4oeSMQcqAToKX4%2BVY%2FoKwHPZ3LL3fdWqV0S%2FvWmetYHl%2BXtIMfr4sJ5HoKPeEGaXWKkENsUVjCs33ftb%2Bk6Vh68XGlO5A7hLzsl2zmozVHKtnVHMqNjuSl%2FVTLUSxGOrSXMajdQMItHxDOD4gI5oZA%2FrQy55rsm3Yy%2BuTl0%2FnRrfHed0TzZAp%2F%2By2dFmxusO8axFlSjvdrqSAJF9oAESNvpV6G124LKs01uIQT%2BzPLtwgDb4ZnV8AzgWlnJDQlBhudEBAhKHZIsMbDqQKObxt6eBSEoHlSQ0h6eQsjG3FU1EY8C1%2F0GGZDF0VWO2oYTcspg%2FQJIEo7yz4CR037atiIVgEYIzEhkb9KlYZmye7%2FnJk8Wqab3FraRKRA4NH4SMkbr0ssfhDNp48vrt1aToLK%2FxuYkxeDUJ2jyEChuCcZNAzb5On4rHEP9HywghYEYPSSOqNVqe2KgYnr%2BNJRx9At%2BumCzYS2uPoXq%2FdVJByyB7RsbMXUq2lzc8dt95PO%2BkCFfVa1MgDkVN3T3jY%2FKQIBPK12FXYyMGO0%2F5KPgqmM77ItwinSXDlQBQcTHG5JyTAMHmjunt2bP%2Fj%2FgPFOe%2F%2Baf%2FiQDbqNtvcnRfkH2ohjdh6dUQ5m%2B2VTNYWkGxattRg9EKKE5vey1FPRlCqZyDgvrv9lvrlshrlzdNlGwotAGLfLMcs2vyw1rETiNehQSuxs4fVNVenj9bBwNMtd92XoZ0fgAFx0VYRpgUubx6SaFj1P1ns1k7saP20CtOv40Vh1fIBS7G3r7SNtlzk3C0A%2F4Rmw2%2BXo6xnkHUnscHL5GCzjGbRjxavMDrnvi92ihmau6VuJALuZvH2%2BXItM49krxVbbQbx%2BrvmYdNFMYrxrqfXKoG%2FeBbQYV684ncHXIr7RWAGAEkKq%2FpI3%2Bjw0KrRp1bA%3D%3D&ctl00%24txtAutoComplete=&ctl00%24BodyABC%24$GROUPNAME=on&ctl00%24BodyABC%24Button1=T%C3%A9l%C3%A9charger";
                    postData = postData.Replace("$GROUPNAME", groupName);
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

                    success = SaveResponseToFile(fileName, req);
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
                if (File.Exists(fileName) && destFolder.Contains("archive"))
                    return true;

                // Send POST request
                string url = "https://www.abcbourse.com/download/historiques.aspx";


                // Get ViewState 
                using (WebClient webClient = new WebClient())
                {
                    webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36");
                    byte[] response = webClient.DownloadData(url);

                    string htmlContent = Encoding.ASCII.GetString(response);
                    dailyViewState = ExtractValue(htmlContent, "__VIEWSTATE");
                    dailyViewStateGenerator = ExtractValue(htmlContent, "__VIEWSTATEGENERATOR");
                    dailyEventValidation = ExtractValue(htmlContent, "__EVENTVALIDATION");
                }

                string postData = "ctl00_BodyABC_ToolkitScriptManager1_HiddenField=%3B%3BAjaxControlToolkit%2C+Version%3D3.0.20229.20843%2C+Culture%3Dneutral%2C+PublicKeyToken%3D28f01b0e84b6d53e%3Afr-FR%3A3b7d1b28-161f-426a-ab77-b345f2c428f5%3A865923e8%3A9b7907bc%3A411fea1c%3Ae7c87f07%3A91bd373d%3Abbfda34c%3A30a78ec5%3A9349f837%3Ad4245214%3A77c58d20%3A14b56adc%3A8e72a662%3Aacd642d2%3A596d588c%3A269a19ae&"
                    + "__EVENTTARGET=&"
                    + "__EVENTARGUMENT=&"
                    + "__VIEWSTATE=" + dailyViewState + "&"
                    + "__VIEWSTATEGENERATOR=" + dailyViewStateGenerator + "&"
                    + "__EVENTVALIDATION=" + dailyEventValidation + "&"
                    + "ctl00%24BodyABC%24strDateDeb=$START_DAY%2F$START_MONTH%2F$START_YEAR&"
                    + "ctl00%24BodyABC%24strDateFin=$END_DAY%2F$END_MONTH%2F$END_YEAR&"
                    + "ctl00%24BodyABC%24$ABCGROUP=on&"
                    + "ctl00%24BodyABC%24Button1=T%C3%A9l%C3%A9charger&"
                    + "ctl00%24BodyABC%24dlFormat=x&"
                    + "ctl00%24BodyABC%24listFormat=isin";


                //ctl00$BodyABC$strDateDeb:01/02/2017
                //ctl00$BodyABC$strDateFin:20/02/2017
                //ctl00$BodyABC$eurolistap:on
                //ctl00$BodyABC$txtOneSico:
                //ctl00$BodyABC$Button1:Télécharger
                //ctl00$BodyABC$dlFormat:w
                //ctl00$BodyABC$listFormat:isin




                postData = postData.Replace("$ABCGROUP", abcGroup);

                postData = postData.Replace("$START_DAY", "01");
                postData = postData.Replace("$START_MONTH", month.Month.ToString("00"));
                postData = postData.Replace("$START_YEAR", month.Year.ToString());
                postData = postData.Replace("$END_DAY", DateTime.DaysInMonth(month.Year, month.Month).ToString());
                postData = postData.Replace("$END_MONTH", month.Month.ToString("00"));
                postData = postData.Replace("$END_YEAR", month.Year.ToString());

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

                success = SaveResponseToFile(fileName, req);
            }
            catch (System.Exception ex)
            {
                StockLog.Write(ex);
                System.Windows.Forms.MessageBox.Show(ex.Message, "Connection failed");
                success = false;
            }
            return success;
        }
        private bool DownloadFileFromProvider2(string destFolder, string fileName, DateTime startDate, DateTime endDate, string ISIN)
        {
            bool success = true;
            try
            {
                // Send POST request
                string url = "https://www.abcbourse.com/download/historiques.aspx";
                if (dailyViewState == string.Empty)
                {
                    // Get ViewState 
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/31.0.1650.63 Safari/537.36");
                        byte[] response = webClient.DownloadData(url);

                        string htmlContent = Encoding.ASCII.GetString(response);
                        dailyViewState = ExtractValue(htmlContent, "__VIEWSTATE");
                        dailyViewStateGenerator = ExtractValue(htmlContent, "__VIEWSTATEGENERATOR");
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

        private bool DownloadFileFromProvider(string destFolder, string fileName, DateTime startDate, DateTime endDate, string ISIN)
        {
            bool success = true;
            try
            {
                // Send POST request
                //string url = "http://www.abcbourse.com/download/historiques.aspx";
                string url = "https://www.abcbourse.com/download/download.aspx?s=" + ISIN;
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
                    + "ctl00%24BodyABC%24txtFrom=$START_DAY%2F$START_MONTH%2F$START_YEAR&"
                    + "ctl00%24BodyABC%24txtTo=$END_DAY%2F$END_MONTH%2F$END_YEAR&"
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

        public List<StockSerie> DownloadLabels()
        {
            List<StockSerie> series = new List<StockSerie>();
            try
            {
                // Build post data
                ASCIIEncoding encoding = new ASCIIEncoding();

                // Send POST request
                string url = "https://www.abcbourse.com/download/libelles.aspx";

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
                    + "ctl00%24BodyABC%24indicessecp=on&"
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
                            if (!line.StartsWith("#"))
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
                            if (!line.StartsWith("#"))
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
                            if (!line.StartsWith("#"))
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


        //public static void DownloadFinancial2(StockSerie stockSerie)
        //{
        //    if (stockSerie.StockAnalysis.Financial != null && stockSerie.StockAnalysis.Financial.DownloadDate.AddDays(7) > DateTime.Now) return;

        //    string url = "http://www.abcbourse.com/analyses/chiffres.aspx?s=$ShortNamep".Replace("$ShortName", stockSerie.ShortName);
        //    url = "http://www.boursorama.com/bourse/profil/profil_finance.phtml?symbole=1rP$ShortName".Replace("$ShortName", stockSerie.ShortName);
        //    StockWebHelper swh = new StockWebHelper();
        //    string html = swh.DownloadHtml(url);

        //    WebBrowser browser = new WebBrowser();
        //    browser.ScriptErrorsSuppressed = true;
        //    browser.DocumentText = html;
        //    browser.Document.OpenNew(true);
        //    browser.Document.Write(html);
        //    browser.Refresh();

        //    HtmlDocument doc = browser.Document;

        //    HtmlElementCollection tables = doc.GetElementsByTagName("div");
        //    List<List<string>> data = new List<List<string>>();

        //    StockFinancial financial = new StockFinancial();

        //    HtmlElement tbl = tables.Cast<HtmlElement>().FirstOrDefault(t => t.InnerText.StartsWith("Marché"));
        //    if (tbl != null)
        //    {
        //        //ParseFinancialGeneral(stockSerie, financial, tbl);
        //    }
        //    bool found = false;
        //    int count = 0;
        //    foreach (HtmlElement table in tables)
        //    {
        //        if (found)
        //        {
        //            switch (count)
        //            {
        //                case 0:
        //                    financial.IncomeStatement = getTableData(table);
        //                    count++;
        //                    break;
        //                case 1:
        //                    financial.BalanceSheet = getTableData(table);
        //                    count++;
        //                    break;
        //                case 2:
        //                    financial.Ratios = getTableData(table);
        //                    count++;
        //                    break;
        //                case 3:
        //                    financial.Quaterly = getTableData(table);
        //                    count++;
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            found = table.InnerText.StartsWith("Compte de");
        //        }
        //    }

        //    if (found)
        //        tbl = tables.Cast<HtmlElement>().FirstOrDefault(t => t.InnerText.StartsWith("Compte"));
        //    if (tbl != null)
        //    {
        //        ParseFinancialDetails(stockSerie, financial, tbl);
        //    }
        //}

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
                string shortName = stockSerie.StockGroup == StockSerie.Groups.ALTERNEXT ? "EP" : "P";
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

        private static void ParseFinancialDetails(StockSerie stockSerie, StockFinancial financial, HtmlElement table)
        {
            HtmlElementCollection tables = table.GetElementsByTagName(("table"));
            foreach (HtmlElement tbl in tables)
            {

            }
        }

        //private static void ParseFinancialGeneral(StockSerie stockSerie, StockFinancial financial, HtmlElement tbl)
        //{
        //    List<List<string>> data = getTableData(tbl);
        //    foreach (var row in data)
        //    {
        //        if (row.Count == 2)
        //        {
        //            switch (row[0].Trim())
        //            {
        //                case "Marché":
        //                    financial.Market = row[1];
        //                    break;
        //                case "Nombre de titres":
        //                    financial.ShareNumber = long.Parse(row[1].Replace(" ", ""));
        //                    break;
        //                case "Place de cotation":
        //                    financial.MarketPlace = row[1];
        //                    break;
        //                case "Secteur d'activité":
        //                    financial.Sector = row[1];
        //                    break;
        //                case "Eligible au SRD":
        //                    financial.SRD = row[1];
        //                    break;
        //                case "Eligible au PEA":
        //                    financial.PEA = row[1];
        //                    break;
        //                case "Indices":
        //                    financial.Indices = row[1];
        //                    break;
        //                case "Capitalisation (milliers d'euros)":
        //                    //financial.MarketCap = int.Parse(row[1].Replace(" ", ""));
        //                    break;
        //                case "Rendement":
        //                    float yield = 0f;
        //                    if (float.TryParse(row[1].Replace(",", ".").Replace("%", ""), out yield))
        //                    {
        //                        financial.Yield = yield / 100f;
        //                    }
        //                    break;
        //                case "Dividende (Date de versement)":
        //                    financial.Dividend = row[1];
        //                    break;
        //                case "Date Assemblée Générale":
        //                    financial.MeetingDate = row[1];
        //                    break;
        //            }
        //        }
        //        financial.DownloadDate = DateTime.Now;
        //        stockSerie.StockAnalysis.Financial = financial;
        //    }
        //}

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

            string url = "http://www.abcbourse.com/marches/events.aspx?s=$ShortNamep".Replace("$ShortName",
               stockSerie.ShortName);

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
    }
}

