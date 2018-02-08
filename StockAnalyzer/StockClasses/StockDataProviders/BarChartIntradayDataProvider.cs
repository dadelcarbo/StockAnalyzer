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
    public class BarChartIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\BarChartIntraday";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\BarChartIntraday";
        static private readonly string CONFIG_FILE = @"\BarChartIntradayDownload.cfg";
        static private readonly string CONFIG_FILE_USER = @"\BarChartIntradayDownload.user.cfg";

        public string UserConfigFileName => CONFIG_FILE_USER;

        public override bool LoadIntradayDurationArchiveData(string rootFolder, StockSerie serie, StockSerie.StockBarDuration duration)
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

                        if (File.Exists(durationFileName) &&
                            File.GetLastWriteTime(durationFileName).Date == DateTime.Today.Date) break; // Only cache once a day.
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
            var interval = 5;
            var from = (long)((startDate - refDate).TotalSeconds);
            var to = (long)((DateTime.Now - refDate).TotalSeconds);

            var code = mapping[symbol];

            //    https://tvc4.forexpros.com/ff7ac8140917544c3e1d9f93fef42180/1516029580/1/1/8/history?symbol=1&resolution=5&from=1515597598&to=1516029658

            return $"https://tvc4.forexpros.com/ff7ac8140917544c3e1d9f93fef42180/1516029580/1/1/8/history?symbol={code}&resolution={interval}&from={from}&to={to}";
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
                    var url = FormatIntradayURL(stockSerie.ShortName, DateTime.Today.AddDays(-20));

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

        private static readonly SortedDictionary<string, string> mapping = new SortedDictionary<string, string>();

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
                            var stockSerie = new StockSerie(row[2], shortName, (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[3]), StockDataProvider.BarChartIntraday);

                            if (!stockDictionary.ContainsKey(row[1]))
                            {
                                stockDictionary.Add(row[2], stockSerie);
                                if (mapping.ContainsKey(shortName))
                                {
                                    MessageBox.Show($"Duplicate entry\r\n erroneous line: {line}",
                                        @"Error in Bar Chart Intraday config file", MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                }
                                else
                                {
                                    mapping.Add(shortName, row[0]);
                                }
                            }
                            else
                            {
                                StockLog.Write("BarChart Entry: " + row[2] + " already in stockDictionary");
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

        static DateTime refDate = new DateTime(1970, 01, 01) + (DateTime.Now - DateTime.UtcNow);
        private static bool ParseIntradayData(StockSerie stockSerie, string fileName)
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

                        var openDate = refDate.AddSeconds(barchartJson.T[i]);
                        if (!stockSerie.ContainsKey(openDate))
                        {
                            var dailyValue = new StockDailyValue(stockSerie.StockName,
                                   barchartJson.O[i],
                                   barchartJson.H[i],
                                   barchartJson.L[i],
                                   barchartJson.C[i],
                                   0,
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
                StockLog.Write("Unable to parse intraday data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            //BarChartDataProviderConfigDlg configDlg = new BarChartDataProviderConfigDlg(stockDico);
            //return configDlg.ShowDialog();
            throw new NotImplementedException();
        }

        public string DisplayName => "CommerzBank Intraday";
    }
}
