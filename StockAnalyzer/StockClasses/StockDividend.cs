﻿using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockWeb;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses
{
    public class StockDividendEntry
    {
        public DateTime Date { get; set; }
        public float Dividend { get; set; }
    }

    public class StockDividend
    {
        public StockDividend(StockSerie stockSerie)
        {
            this.Entries = new List<StockDividendEntry>();
            var shortName = stockSerie.Symbol;
            if (stockSerie.DataProvider == StockDataProvider.ABC)
            {
                shortName += ".PA";
            }
            var filePath = Path.Combine(Folders.DividendFolder, shortName + ".csv");
            if (File.Exists(filePath))
            {
                this.LoadFromFile(filePath);
            }
            else
            {
                DownloadDate = DateTime.MinValue;
            }
        }

        private void LoadFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                DownloadDate = File.GetLastWriteTimeUtc(filePath);
                var entries = new List<StockDividendEntry>();
                foreach (var line in File.ReadAllLines(filePath).Skip(1))
                {
                    try
                    {
                        var fields = line.Split(',');
                        entries.Add(new StockDividendEntry { Date = DateTime.Parse(fields[0]), Dividend = float.Parse(fields[1]) });
                    }
                    catch { }
                }

                this.Entries = entries.OrderBy(e => e.Date).ToList();
            }
        }

        public DateTime DownloadDate { get; set; }
        public List<StockDividendEntry> Entries { get; set; }

        public bool ContainsKey(DateTime date)
        {
            return Entries.Any(e => e.Date == date);
        }
        public void Add(DateTime date, float dividend)
        {
            Entries.Add(new StockDividendEntry { Date = date, Dividend = dividend });
        }
        public StockDividendEntry this[DateTime date]
        {
            get
            {
                return this.Entries.First(e => e.Date == date);
            }
        }

        public bool DownloadFromYahoo(StockSerie stockSerie, bool force = false)
        {
            var shortName = stockSerie.Symbol;
            if (stockSerie.DataProvider == StockDataProvider.ABC && stockSerie.BelongsToGroup(StockSerie.Groups.PEA))
            {
                if (!shortName.EndsWith(".PA"))
                {
                    shortName += ".PA";
                }
            }
            else if (stockSerie.DataProvider != StockDataProvider.Yahoo)
            {
                return false;
            }
            var filePath = Path.Combine(Folders.DividendFolder, shortName + ".csv");
            if (!force && File.Exists(filePath) && File.GetLastWriteTimeUtc(filePath) > DateTime.Today.AddMonths(-1))
                return false;

            var startDate = new DateTime(2000, 1, 1);
            DateTime refDate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var startSecond = (startDate - refDate).TotalSeconds;
            var endSecond = (DateTime.Today - refDate).TotalSeconds;

            var url = $"https://query1.finance.yahoo.com/v7/finance/download/{shortName}?period1={startSecond}&period2={endSecond}&interval=1mo&events=div";
            var webHelper = new StockWebHelper();

            this.DownloadDate = DateTime.Today;
            if (webHelper.DownloadFile(Folders.DividendFolder, shortName + ".csv", url))
            {
                this.LoadFromFile(filePath);
                return true;
            }
            return false;
        }
    }
}
