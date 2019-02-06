using System;
using System.IO;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class RatioDataProvider : StockDataProviderBase
    {

        static private string FOLDER = @"\data\daily\Ratio";
        static private string ARCHIVE_FOLDER = @"\data\archive\daily\Ratio";

        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            //return;
            string line;
            string fileName = rootFolder + "\\RatioIndicator.txt";
            // Parse yahoo.cfg file// Create data folder if not existing
            if (!Directory.Exists(rootFolder + FOLDER))
            {
                Directory.CreateDirectory(rootFolder + FOLDER);
            }
            if (!Directory.Exists(rootFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(rootFolder + ARCHIVE_FOLDER);
            }

            if (File.Exists(fileName))
            {
                // Parse RatioIndicator.txt file
                using (StreamReader sr = new StreamReader(fileName, true))
                {
                    sr.ReadLine(); // Skip first line
                    while (!sr.EndOfStream)
                    {
                        line = sr.ReadLine();
                        if (!line.StartsWith("#") && !string.IsNullOrWhiteSpace(line))
                        {
                            string[] row = line.Split(',');
                            string serieName = row[0] + "/" + row[1];
                            if (!stockDictionary.ContainsKey(serieName))
                            {
                                if (!stockDictionary.ContainsKey(row[0]))
                                {
                                    throw new Exception("Stock " + row[0] + " Not found, cannot calculate ratio");
                                }
                                StockSerie s1 = stockDictionary[row[0]];
                                if (!stockDictionary.ContainsKey(row[1]))
                                {
                                    throw new Exception("Stock " + row[1] + " Not found, cannot calculate ratio");
                                }
                                StockSerie s2 = stockDictionary[row[1]];

                                stockDictionary.Add(serieName, new StockSerie(serieName, serieName, StockSerie.Groups.RATIO, StockDataProvider.Ratio));
                            }
                        }
                    }
                }
            }
        }

        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            string[] row = stockSerie.StockName.Split('/');
            string serieName = row[0] + "/" + row[1];
            if (!StockDictionary.StockDictionarySingleton.ContainsKey(row[0]))
            {
                throw new Exception("Stock " + row[0] + " Not found, cannot calculate ratio");
            }
            StockSerie s1 = StockDictionary.StockDictionarySingleton[row[0]];
            if (!StockDictionary.StockDictionarySingleton.ContainsKey(row[1]))
            {
                throw new Exception("Stock " + row[1] + " Not found, cannot calculate ratio");
            }
            StockSerie s2 = StockDictionary.StockDictionarySingleton[row[1]];

            // Generate ratio serie
            StockSerie s3 = s1.GenerateRelativeStrenthStockSerie(s2);

            // Copy Into current serie
            stockSerie.IsInitialised = false;
            foreach (var pair in s3)
            {
                stockSerie.Add(pair.Key, pair.Value);
            }
            return true;
        }


        public override bool SupportsIntradayDownload
        {
            get { return false; }
        }

        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            return false;
        }

    }
}
