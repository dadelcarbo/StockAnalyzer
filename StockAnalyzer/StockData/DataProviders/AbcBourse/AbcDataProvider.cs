using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerApp.StockData;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        protected override void PreInitDictionary(bool download)
        {
            ABC_WEB_CACHE_FOLDER = Path.Combine(DataFolder, "WebCache");
            if (!Directory.Exists(Path.Combine(DataFolder, ABC_WEB_CACHE_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(DataFolder, ABC_WEB_CACHE_FOLDER));
            }

            ABC_TMP_FOLDER = Path.Combine(DataFolder, "Tmp");
            if (!Directory.Exists(Path.Combine(DataFolder, ABC_TMP_FOLDER)))
            {
                Directory.CreateDirectory(Path.Combine(DataFolder, ABC_TMP_FOLDER));
            }
            else
            {
                foreach (string file in Directory.GetFiles(Path.Combine(DataFolder, ABC_TMP_FOLDER)))
                {
                    // Purge files at each start
                    File.Delete(file);
                }
            }


            if (File.Exists(excludeFileName))
            {
                excludeList = File.ReadAllLines(excludeFileName).ToList();
            }
        }
        /// <summary>
        /// Creates a StockInstrument from a configuration line. Format: FR0003500008;CAC40;;PX1;INDICES Format:
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

                var instrument = new StockInstrument()
                {
                    Id = row[0],
                    Name = row[1],
                    Isin = row[0],
                    Symbol = row[3],
                    Provider = DataProvider.ABC,
                    Group = (Groups)Enum.Parse(typeof(Groups), row[4])
                };
                instrument.AbcId = instrument.Symbol + instrument.Isin?.Substring(0, 2) switch
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

                return instrument;
            }
            catch (Exception ex)
            {
                StockLog.Write($"Line: {line}");
                StockLog.Write(ex.Message);
            }
            return null;
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
    }
}
