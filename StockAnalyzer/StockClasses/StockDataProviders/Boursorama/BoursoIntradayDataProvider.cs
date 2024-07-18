using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders.Bourso
{
    public class BoursoIntradayDataProvider : StockDataProviderBase, IConfigDialog
    {
        private static readonly string FOLDER = @"\intraday\BoursoIntraday";
        private static readonly string ARCHIVE_FOLDER = @"\archive\intraday\BoursoIntraday";

        private static readonly string CONFIG_FILE = "BoursoIntradayDownload.cfg";


        public string UserConfigFileName => CONFIG_FILE;

        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            // Parse BoursoIntraday.cfg file// Create data folder if not existing
            if (!Directory.Exists(DataFolder + FOLDER))
            {
                Directory.CreateDirectory(DataFolder + FOLDER);
            }
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }

            this.needDownload = download;

            // Load Config files
            if (download)
            {
                InitFromFile(Path.Combine(Folders.PersonalFolder, UserConfigFileName), stockDictionary);
            }
        }

        private void InitFromFile(string fileName, StockDictionary stockDictionary)
        {
            if (File.Exists(fileName))
            {
                using StreamReader sr = new StreamReader(fileName, true);
                string line;
                sr.ReadLine(); // Skip first line
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (line.StartsWith("$"))
                        break;

                    if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                    {
                        string[] row = line.Split(';');
                        if (row.Length == 3)
                        {
                            if (stockDictionary.ContainsKey(row[1]))
                            {
                                var stockSerie = stockDictionary[row[1]];
                                if (stockSerie.ISIN == row[0] && stockSerie.Symbol == row[2])
                                {
                                    var archiveFileName = DataFolder + ARCHIVE_FOLDER + $"\\{stockSerie.Symbol}_{stockSerie.StockGroup}.txt";
                                    if (!File.Exists(archiveFileName) || File.GetLastWriteTime(archiveFileName) < DateTime.Today.AddDays(-3))
                                    {
                                        if (DownloadDailyData(stockSerie))
                                        {
                                            stockSerie.BarDuration = BarDuration.M_5;
                                            LoadData(stockSerie);
                                            stockSerie.BarDuration = BarDuration.Daily;
                                        }
                                    }
                                }
                                else
                                {
                                    MessageBox.Show($"Inconsistent config: {line}", "Boursorama intraday error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Inconsistent config: {line}, 3 fields expected", "Boursorama intraday error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        public override bool SupportsIntradayDownload => true;

        public override bool LoadData(StockSerie stockSerie)
        {
            StockLog.Write("LoadData for " + stockSerie.StockName);
            bool res = false;
            var archiveFileName = DataFolder + ARCHIVE_FOLDER + $"\\{stockSerie.Symbol}_{stockSerie.StockGroup}.txt";
            if (File.Exists(archiveFileName))
            {
                if (File.GetLastWriteTime(archiveFileName) > DateTime.Today.AddDays(-7))
                {
                    stockSerie.ReadFromCSVFile(archiveFileName);
                    res = true;
                }
            }

            var fileName = DataFolder + FOLDER + $"\\{stockSerie.Symbol}_{stockSerie.StockGroup}.txt";

            if (File.Exists(fileName))
            {
                if (ParseBoursoData(stockSerie, fileName))
                {
                    var lastDate = stockSerie.Keys.Last();

                    // Clean data gaps
                    var startDate = lastDate;
                    var gap = new TimeSpan(4, 0, 0, 0);
                    foreach (var date in stockSerie.Keys.Reverse())
                    {
                        if (startDate - date > gap)
                            break;
                        startDate = date;
                    }

                    stockSerie.SaveToCSVFromDateToDate(archiveFileName, startDate, lastDate);
                    File.Delete(fileName);
                }
                else
                {
                    return false;
                }
                res = true;
            }
            return res;
        }

        public string FormatURL(StockSerie stockSerie)
        {
            if (string.IsNullOrEmpty(stockSerie.Symbol) || string.IsNullOrEmpty(stockSerie.ISIN) || stockSerie.ISIN.Length < 2)
                return null;
            var prefix = stockSerie.ISIN.Substring(0, 2);
            if (stockSerie.StockGroup == StockSerie.Groups.FUND)
            {
                prefix += "T";
            }
            var symbol = prefix switch
            {
                "FR" => $"1rP{stockSerie.Symbol}",
                "FRT" => $"1rT{stockSerie.Symbol}",
                "DE" => $"1z{stockSerie.Symbol}",
                "BE" => $"FF11-{stockSerie.Symbol}",
                "NL" => $"1rA{stockSerie.Symbol}",
                "LU" => $"1rA{stockSerie.Symbol}",
                "PT" => $"1rL{stockSerie.Symbol}",
                "IT" => $"1g{stockSerie.Symbol}",
                "ES" => $"FF55-{stockSerie.Symbol}",
                "US" => $"{stockSerie.Symbol}",
                "CA" => $"{stockSerie.Symbol}",
                _ => null
            };
            if (stockSerie.BelongsToGroup(StockSerie.Groups.EURO_A_B_C) && prefix != "LU")
            {
                symbol = $"1rP{stockSerie.Symbol}";
            }
            if (symbol == null)
                return null;
            return $"https://www.boursorama.com/bourse/action/graph/ws/GetTicksEOD?symbol={symbol}&length=5&period=-1&guid=";
        }

        public override bool ForceDownloadData(StockSerie stockSerie)
        {
            StockLog.Write("ForceDownloadData for " + stockSerie.StockName);
            var archiveFileName = DataFolder + ARCHIVE_FOLDER + $"\\{stockSerie.Symbol}_{stockSerie.StockGroup}.txt";
            if (File.Exists(archiveFileName))
            {
                File.Delete(archiveFileName);
            }
            var fileName = DataFolder + FOLDER + $"\\{stockSerie.Symbol}_{stockSerie.StockGroup}.txt";
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            stockSerie.IsInitialised = false;
            return this.DownloadDailyData(stockSerie);
        }

        static TimeSpan marketClose = new TimeSpan(17, 35, 00);
        static readonly SortedDictionary<string, DateTime> DownloadHistory = new SortedDictionary<string, DateTime>();

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            StockLog.Write("DownloadDailyData for " + stockSerie.StockName);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading intraday data for " + stockSerie.StockName);

                if (DownloadHistory.ContainsKey(stockSerie.ISIN) && DownloadHistory[stockSerie.ISIN] > DateTime.Now.AddMinutes(-2))
                {
                    return false;  // Do not download more than every 2 minutes.
                }

                if (stockSerie.Initialise() && stockSerie.Count > 0)
                {
                    if (stockSerie.LastValue.DATE.Date == DateTime.Today && stockSerie.LastValue.DATE.TimeOfDay >= marketClose)
                    {
                        return false; // uptodate for today
                    }
                }

                var url = FormatURL(stockSerie);
                if (url == null)
                    return false;
                var fileName = DataFolder + FOLDER + $"\\{stockSerie.Symbol}_{stockSerie.StockGroup}.txt";

                using var wc = new WebClient();
                wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                int nbTries = 2;
                while (nbTries > 0)
                {
                    try
                    {
                        var client = new HttpClient();
                        var response = client.GetStringAsync(url).Result;
                        if (!string.IsNullOrEmpty(response))
                        {
                            if (response.StartsWith("{"))
                            {
                                var date = DateTime.Now.TimeOfDay > new TimeSpan(17, 40, 0) ? DateTime.Today.AddMinutes(1439) : DateTime.Now; // Set to 23h59 after 17h40 to prevent for download
                                if (DownloadHistory.ContainsKey(stockSerie.ISIN))
                                {
                                    DownloadHistory[stockSerie.ISIN] = date;
                                }
                                else
                                {
                                    DownloadHistory.Add(stockSerie.ISIN, date);
                                }
                                File.WriteAllText(fileName, response);
                                stockSerie.IsInitialised = false;
                                return true;
                            }
                            StockLog.Write(response);
                            return false;
                        }
                        StockLog.Write(response);
                        nbTries--;
                    }
                    catch (Exception ex)
                    {
                        nbTries--;
                        StockLog.Write(ex);
                    }
                }
            }
            return false;
        }

        static private DateTime BoursoIntradayDateToDateTime(long d)
        {
            var dateString = d.ToString();

            var year = 2000 + int.Parse(dateString.Substring(0, 2));
            var month = int.Parse(dateString.Substring(2, 2));
            var day = int.Parse(dateString.Substring(4, 2));

            var minutes = int.Parse(dateString.Substring(6, 4));

            return new DateTime(year, month, day).AddMinutes(minutes);
        }

        private static bool ParseBoursoData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using var sr = new StreamReader(fileName);
                var boursoData = JsonSerializer.Deserialize<BoursoJson>(sr.ReadToEnd());
                if (string.IsNullOrEmpty(boursoData?.d?.Name))
                {
                    StockLog.Write($"Error loading {stockSerie.StockName}");
                    return false;
                }

                DateTime lastDate = stockSerie.Count > 0 ? stockSerie.Keys.Last() : DateTime.MinValue;

                var bars = boursoData?.d?.QuoteTab?.Select(d => new StockDailyValue(d.o, d.h, d.l, d.c, d.v, BoursoIntradayDateToDateTime(d.d))).Where(b => b.DATE >= lastDate).ToList();
                if (bars.Count > 0)
                {
                    bars = FixMinuteBars(bars, 1);

                    foreach (var bar in bars)
                    {
                        stockSerie.Add(bar.DATE, bar);
                    }
                }
                stockSerie.ClearBarDurationCache();
                res = true;
            }
            catch (Exception e)
            {
                StockLog.Write("Unable to parse daily data for " + stockSerie.StockName);
                StockLog.Write(e);
            }
            return res;
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            return DialogResult.OK;
        }

        public override string DisplayName => "Bourso Intraday";

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            Process.Start($"https://finance.yahoo.com/quote/{stockSerie.Symbol}");
        }

        public override void ApplyTrim(StockSerie stockSerie, DateTime Date)
        {
            if (!stockSerie.Initialise())
                return;

            var archiveFileName = DataFolder + ARCHIVE_FOLDER + $"\\{stockSerie.Symbol}_{stockSerie.StockGroup}.txt";
            stockSerie.SaveToCSVFromDateToDate(archiveFileName, Date, stockSerie.LastValue.DATE);
        }


        static string[] configLines;
        public static void AddSerie(StockSerie serie)
        {
            if (!ContainsSerie(serie))
            {
                configLines = configLines.Append($"{serie.ISIN};{serie.StockName};{serie.Symbol}").ToArray();
                File.WriteAllLines(Path.Combine(Folders.PersonalFolder, CONFIG_FILE), configLines);
            }
        }
        public static void RemoveSerie(StockSerie serie)
        {
            if (ContainsSerie(serie))
            {
                configLines = configLines.Where(x => !x.Contains(serie.ISIN)).ToArray();
                File.WriteAllLines(Path.Combine(Folders.PersonalFolder, CONFIG_FILE), configLines);
            }
        }
        public static bool ContainsSerie(StockSerie serie)
        {
            configLines ??= File.ReadAllLines(Path.Combine(Folders.PersonalFolder, CONFIG_FILE));
            return !string.IsNullOrEmpty(serie.ISIN) && configLines.Any(l => l.Contains(serie.ISIN));
        }

    }
}
