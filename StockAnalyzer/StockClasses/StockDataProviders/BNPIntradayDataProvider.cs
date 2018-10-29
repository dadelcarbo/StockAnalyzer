using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using System.Collections.Generic;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class BNPIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\BNPIntraday";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\BNPIntraday";
        static private readonly string CONFIG_FILE = @"\BNPIntradayDownload.cfg";
        static private readonly string CONFIG_FILE_USER = @"\BNPIntradayDownload.user.cfg";

        public string UserConfigFileName => CONFIG_FILE_USER;

        public override bool LoadIntradayDurationArchiveData(string rootFolder, StockSerie serie, StockBarDuration duration)
        {
            StockLog.Write("LoadIntradayDurationArchiveData Name:" + serie.StockName + " duration:" + duration);
            var durationFileName = rootFolder + ARCHIVE_FOLDER + "\\" + duration + "\\" + serie.ShortName.Replace(':', '_') + "_" + serie.StockName + "_" + serie.StockGroup.ToString() + ".txt";
            if (File.Exists(durationFileName))
            {
                var values = serie.GetValues(duration);
                if (values == null)
                    StockLog.Write("LoadIntradayDurationArchiveData Cache File Found, current size is: 0");
                else StockLog.Write("LoadIntradayDurationArchiveData Cache File Found, current size is: " + values.Count);
                serie.ReadFromCSVFile(durationFileName, duration);


                StockLog.Write("LoadIntradayDurationArchiveData New serie size is: " + serie.GetValues(duration).Count);
                if (serie.GetValues(duration).Count > 0)
                {
                    StockLog.Write("LoadIntradayDurationArchiveData First bar: " +
                                   serie.GetValues(duration).First().ToString());
                    StockLog.Write("LoadIntradayDurationArchiveData Last bar: " + serie.GetValues(duration).Last().ToString());
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            // Create data folder if not existing
            if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
            }

            foreach (var duration in cacheDurations)
            {
                var durationFileName = rootFolder + ARCHIVE_FOLDER + "\\" + duration;
                if (!Directory.Exists(durationFileName))
                {
                    Directory.CreateDirectory(durationFileName);
                }
            }

            if (!Directory.Exists(rootFolder + INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + INTRADAY_FOLDER);
            }
            else
            {
                foreach (var file in Directory.GetFiles(rootFolder + INTRADAY_FOLDER))
                {
                    if (File.GetLastWriteTime(file).Date != DateTime.Today)
                    {
                        File.Delete(file);
                    }
                }
            }

            // Parse CommerzBankDownload.cfg file
            this.needDownload = download;
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE);
            InitFromFile(rootFolder, stockDictionary, download, rootFolder + CONFIG_FILE_USER);

        }
        public override bool SupportsIntradayDownload => true;

        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            var archiveFileName = rootFolder + ARCHIVE_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(archiveFileName))
            {
                stockSerie.ReadFromCSVFile(archiveFileName);
            }

            var fileName = rootFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

            if (File.Exists(fileName))
            {
                if (ParseIntradayData(stockSerie, fileName))
                {
                    stockSerie.Values.Last().IsComplete = false;
                    var lastDate = stockSerie.Keys.Last();

                    var firstArchiveDate = lastDate.AddMonths(-2).AddDays(-lastDate.Day + 1).Date;

                    stockSerie.SaveToCSVFromDateToDate(archiveFileName, firstArchiveDate, lastDate.AddDays(-5).Date);

                    // Archive other time frames
                    string durationFileName;
                    var previousDuration = stockSerie.BarDuration;
                    foreach (var duration in cacheDurations)
                    {
                        durationFileName = rootFolder + ARCHIVE_FOLDER + "\\" + duration + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                        if (File.Exists(durationFileName) && File.GetLastWriteTime(durationFileName).Date == DateTime.Today.Date) break; // Only cache once a day.
                        stockSerie.BarDuration = duration;
                        stockSerie.SaveToCSVFromDateToDate(durationFileName, stockSerie.Keys.First(), lastDate.AddDays(-1).Date);
                    }

                    // Set back to previous duration.
                    stockSerie.BarDuration = previousDuration;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        static string BARCHART_API_KEY = "ebc71dae1c7ca3157e243600383649e7";

        public string FormatIntradayURL(string symbol, DateTime startDate)
        {
            return $"https://www.produitsdebourse.bnpparibas.fr/productdetailchart/getdata?currentCulture=fr-FR&instrument={symbol}&chartPeriod=OneWeek&chartType=area&exchange=BNP";
        }

        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            var fileName = rootFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName + "_" + stockSerie.StockName + "_" +
                              stockSerie.StockGroup.ToString() + ".txt";
            if (File.Exists(fileName))
            {
                var fileDate = File.GetLastWriteTime(fileName);
                if (fileDate.Date == DateTime.Today)
                    return false;
            }
            this.DownloadIntradayData(rootFolder, stockSerie);
            return true;
        }
        public override bool DownloadIntradayData(string rootFolder, StockSerie stockSerie)
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday for " + stockSerie.StockName);

                var fileName = rootFolder + INTRADAY_FOLDER + "\\" + stockSerie.ShortName.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";

                if (File.Exists(fileName))
                {
                    if (File.GetLastWriteTime(fileName) > DateTime.Now.AddMinutes(-2))
                        return false;
                }
                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    var url = FormatIntradayURL(stockSerie.ISIN, DateTime.Today.AddDays(-30));

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

        private void InitFromFile(string rootFolder, StockDictionary stockDictionary, bool download, string fileName)
        {
            string line;
            if (File.Exists(fileName))
            {
                using (var sr = new StreamReader(fileName, true))
                {
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            var row = line.Split(',');

                            var shortName = row[1];
                            var stockSerie = new StockSerie(row[2], shortName, StockSerie.Groups.TURBO, StockDataProvider.BNPIntraday) {ISIN = row[0]};

                            if (!stockDictionary.ContainsKey(shortName))
                            {
                                stockDictionary.Add(row[2], stockSerie);
                            }
                            else
                            {
                                StockLog.Write("BNP Entry: " + row[2] + " already in stockDictionary");
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

        static DateTime refDate = new DateTime(1970, 01, 01) + (DateTime.Now - DateTime.UtcNow) ;
        private static bool ParseIntradayData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    var json = sr.ReadToEnd();

                    var bnpJson = BNPJSon.FromJson(json);
                    var ticksPerSeconds = TimeSpan.FromSeconds(1).Ticks;

                    var dataSerie = bnpJson.Series.First();
                    foreach (var data in dataSerie.Data)
                    {
                        var openDate = refDate.AddSeconds(data.X/1000);
                        if (!stockSerie.ContainsKey(openDate))
                        {
                            var dailyValue = new StockDailyValue(stockSerie.StockName,
                                   (float)data.Y,
                                   (float)data.Y,
                                   (float)data.Y,
                                   (float)data.Y,
                                   0,
                                   openDate);

                            stockSerie.Add(dailyValue.DATE, dailyValue);
                        }
                    }
                    for (var i = 0; i < bnpJson.Series.Length; i++)
                    {

                        //if (bnpJson.O[i] == 0 && bnpJson.H[i] == 0 && bnpJson.L[i] == 0 && bnpJson.C[i] == 0)
                        //    continue;

                        //var openDate = refDate.AddSeconds(bnpJson.T[i]);
                        //if (!stockSerie.ContainsKey(openDate))
                        //{
                        //    var dailyValue = new StockDailyValue(stockSerie.StockName,
                        //           bnpJson.O[i],
                        //           bnpJson.H[i],
                        //           bnpJson.L[i],
                        //           bnpJson.C[i],
                        //           0,
                        //           openDate);

                        //    stockSerie.Add(dailyValue.DATE, dailyValue);
                        //}
                    }
                    stockSerie.ClearBarDurationCache();

                    res = true;
                }
            }
            catch (System.Exception e)
            {
                StockLog.Write("Unable to parse intraday data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            //BNPDataProviderConfigDlg configDlg = new BNPDataProviderConfigDlg(stockDico);
            //return configDlg.ShowDialog();
            throw new NotImplementedException();
        }

        public string DisplayName => "CommerzBank Intraday";
    }
}
