﻿using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
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
        static private string FOLDER = @"\intraday\BoursoIntraday";
        static private string ARCHIVE_FOLDER = @"\archive\intraday\BoursoIntraday";

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

            // Create serie from EURO_A
            foreach (var stockSerie in stockDictionary.Values.Where(s => s.BelongsToGroup(StockSerie.Groups.EURO_A) || s.BelongsToGroup(StockSerie.Groups.EURO_B) || s.BelongsToGroup(StockSerie.Groups.EURO_C)).ToArray())
            {
                stockDictionary.Add("INT_" + stockSerie.StockName,
                    new StockSerie("INT_" + stockSerie.StockName,
                        stockSerie.Symbol,
                        StockSerie.Groups.INT_EURONEXT,
                        StockDataProvider.BoursoIntraday,
                        BarDuration.Daily) // 1 Minute
                    {
                        ISIN = stockSerie.ISIN
                    });
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
                stockSerie.ReadFromCSVFile(archiveFileName);
                res = true;
            }

            var fileName = DataFolder + FOLDER + $"\\{stockSerie.Symbol}_{stockSerie.StockGroup}.txt";

            if (File.Exists(fileName))
            {
                if (ParseBoursoData(stockSerie, fileName))
                {
                    var lastDate = stockSerie.Keys.Last();

                    stockSerie.SaveToCSVFromDateToDate(archiveFileName, stockSerie.Keys.First(), lastDate);
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

        public string FormatURL(string symbol)
        {
            return $"https://www.boursorama.com/bourse/action/graph/ws/GetTicksEOD?symbol=1rP{symbol}&length=5&period=-1&guid=";
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
            first = true;
            return this.DownloadDailyData(stockSerie);
        }

        static bool first = true;
        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            StockLog.Write("DownloadDailyData for " + stockSerie.StockName);
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NotifyProgress("Downloading daily data for " + stockSerie.StockName);

                var fileName = DataFolder + FOLDER + $"\\{stockSerie.Symbol}_{stockSerie.StockGroup}.txt";

                if (File.Exists(fileName))
                {
                    var lastWriteTime = File.GetLastWriteTime(fileName);
                    if (first && (lastWriteTime > DateTime.Now.AddHours(-2)
                       || (DateTime.Today.DayOfWeek == DayOfWeek.Sunday && lastWriteTime.Date >= DateTime.Today.AddDays(-1))
                       || (DateTime.Today.DayOfWeek == DayOfWeek.Saturday && lastWriteTime.Date >= DateTime.Today)))
                    {
                        if (stockSerie != RefSerie)
                        {
                            first = false;
                            return false;
                        }
                    }
                    else
                    {
                        if (File.GetLastWriteTime(fileName) > DateTime.Now.AddMinutes(-2))
                            return false;
                    }
                }
                using (var wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;

                    var url = string.Empty;
                    var lastDate = new DateTime(ARCHIVE_START_YEAR, 1, 1);
                    if (stockSerie.Initialise() && stockSerie.Count > 0)
                    {
                        lastDate = stockSerie.ValueArray[stockSerie.LastCompleteIndex].DATE.Date;
                        url = FormatURL(stockSerie.Symbol);
                    }
                    else
                    {
                        url = FormatURL(stockSerie.Symbol);
                    }

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
        static private IEnumerable<StockDailyValue> Fix1MinuteBars(IEnumerable<StockDailyValue> bars)
        {
            var previousBar = bars.First();
            var newBars = new List<StockDailyValue>() { previousBar };

            foreach (StockDailyValue bar in bars.Skip(1))
            {
                if (previousBar.DATE.Date == bar.DATE.Date)
                {
                    while ((bar.DATE - previousBar.DATE).Minutes > 1)
                    {
                        previousBar = new StockDailyValue(
                            previousBar.CLOSE,
                            previousBar.CLOSE,
                            previousBar.CLOSE,
                            previousBar.CLOSE,
                            0,
                            previousBar.DATE.AddMinutes(1));
                        newBars.Add(previousBar);
                    }
                }
                newBars.Add(bar);
                previousBar = bar;
            }

            return newBars;
        }

        private static bool ParseBoursoData(StockSerie stockSerie, string fileName)
        {
            var res = false;
            try
            {
                using (var sr = new StreamReader(fileName))
                {
                    var boursoData = JsonSerializer.Deserialize<BoursoJson>(sr.ReadToEnd());
                    if (string.IsNullOrEmpty(boursoData?.d?.Name))
                    {
                        StockLog.Write($"Error loading {stockSerie.StockName}");
                        return false;
                    }

                    DateTime lastDate = stockSerie.Count > 0 ? stockSerie.Keys.Last() : DateTime.MinValue;

                    var bars = boursoData?.d?.QuoteTab?.Select(d => new StockDailyValue(d.o, d.h, d.l, d.c, d.v, BoursoIntradayDateToDateTime(d.d)));
                    bars = Fix1MinuteBars(bars.Where(b => b.DATE >= lastDate));

                    foreach (var bar in bars.Where(b => b.DATE > lastDate))
                    {
                        stockSerie.Add(bar.DATE, bar);
                    }
                    stockSerie.ClearBarDurationCache();
                    res = true;
                }
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
    }
}