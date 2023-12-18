using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.IO;
using System.Linq;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class BreadthDataProvider : StockDataProviderBase
    {
        public override string DisplayName => "Breadth";

        static private string FOLDER = @"\daily\Breadth";
        static private string ARCHIVE_FOLDER = @"\archive\daily\Breadth";

        public static bool NeedGenerate { get; set; }

        /// <summary>
        /// Initialize Breadth serie dictionary
        /// </summary>
        /// <param name="stockDictionary"></param>
        /// <param name="download"></param>
        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            // Parse yahoo.cfg file// Create data folder if not existing
            if (!Directory.Exists(DataFolder + FOLDER))
            {
                Directory.CreateDirectory(DataFolder + FOLDER);
            }
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }
            string line;
            string fileName = Path.Combine(Folders.PersonalFolder, "Breadth.cfg");
            if (File.Exists(fileName))
            {
                // Parse GeneratedIndicator.txt file
                using (StreamReader sr = new StreamReader(fileName, true))
                {
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] row = line.Split(',');
                            string longName = row[0];

                            if (!stockDictionary.ContainsKey(longName))
                            {
                                stockDictionary.Add(longName, new StockSerie(longName, row[0], (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[1]), StockDataProvider.Breadth, BarDuration.Daily));
                                if (longName.StartsWith("AD."))
                                {
                                    var stockName = longName.Replace("AD.", "McClellan.");
                                    stockDictionary.Add(stockName, new StockSerie(stockName, stockName, StockSerie.Groups.BREADTH, StockDataProvider.Breadth, BarDuration.Daily));
                                    stockName = longName.Replace("AD.", "McClellanSum.");
                                    stockDictionary.Add(stockName, new StockSerie(stockName, stockName, StockSerie.Groups.BREADTH, StockDataProvider.Breadth, BarDuration.Daily));
                                }
                            }
                        }
                    }
                }
            }

            NeedGenerate = true;
        }

        public override bool LoadData(StockSerie stockSerie)
        {
            // Read archive first
            string fileName = stockSerie.Symbol + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv";
            string fullFileName = DataFolder + ARCHIVE_FOLDER + "\\" + fileName;
            bool res = ParseCSVFile(stockSerie, fullFileName);

            fullFileName = DataFolder + FOLDER + "\\" + fileName;
            res = ParseCSVFile(stockSerie, fullFileName) || res;
            if (stockSerie.Count > 0)
            {
                stockSerie.PreInitialise();
            }

            var cac40 = StockDictionary.Instance["CAC40"];
            cac40.Initialise();
            if (stockSerie.Count == 0 || cac40.Keys.Last() != stockSerie.LastValue.DATE)
            {
                res |= GenerateBreadthData(stockSerie);
            }
            else
            {
                NeedGenerate = false;
            }
            return res;
        }

        public override bool ForceDownloadData(StockSerie stockSerie)
        {
            stockSerie.IsInitialised = false;
            return this.GenerateBreadthData(stockSerie);
        }

        private bool GenerateBreadthData(StockSerie stockSerie)
        {
            StockLog.Write(stockSerie.StockName);
            var stockDictionary = StockDictionary.Instance;
            string[] row = stockSerie.Symbol.Split('.');
            StockSerie.Groups group = (StockSerie.Groups)Enum.Parse(typeof(StockSerie.Groups), row[1]);
            switch (row[0].Split('_')[0])
            {
                case "AD":
                    return stockDictionary.GenerateAdvDeclSerie(stockSerie, row[1], DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "McClellan":
                    return stockDictionary.GenerateMcClellanSerie(stockSerie, row[1], DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "McClellanSum":
                    return stockDictionary.GenerateMcClellanSumSerie(stockSerie, row[1], DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "EQW":
                    return stockDictionary.GenerateIndiceEqualWeight(stockSerie, row[1], StockBarDuration.Daily, DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "ROC":
                    return stockDictionary.GenerateIndiceBestROC(stockSerie, row[1], StockBarDuration.Daily, DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "OSC":
                    return stockDictionary.GenerateIndiceBestOSC(stockSerie, row[1], StockBarDuration.Daily, DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "HL":
                    return stockDictionary.GenerateHigherThanHLTrailSerie(stockSerie, row[1], StockBarDuration.Daily, DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "EMA":
                    return stockDictionary.GenerateEMABreadthSerie(stockSerie, row[1], StockBarDuration.Daily, DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "STOKF":
                    return stockDictionary.GenerateSTOKFBreadthSerie(stockSerie, row[1], StockBarDuration.Daily, DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "STOK":
                    return stockDictionary.GenerateSTOKBreadthSerie(stockSerie, row[1], StockBarDuration.Daily, DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "RSI":
                    return stockDictionary.GenerateRSIBreadthSerie(stockSerie, row[1], StockBarDuration.Daily, DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "STOKS":
                    return stockDictionary.GenerateSTOKSBreadthSerie(stockSerie, row[1], StockBarDuration.Daily, DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                case "MM":
                    return stockDictionary.GenerateHigherThanMMSerie(stockSerie, row[1], DataFolder + FOLDER, DataFolder + ARCHIVE_FOLDER);
                default:
                    StockLog.Write($"BREADTH Not Found: {stockSerie.StockName}");
                    break;
            }
            return false;
        }
        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            return true;
        }
        public override bool DownloadIntradayData(StockSerie stockSerie)
        {
            return true;
        }
        public override bool SupportsIntradayDownload => false;
    }
}
