using System;
using System.IO;
using System.Linq;
using System.Net;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class FINRADataProvider : StockDataProviderBase
    {
        static private string FOLDER = DAILY_SUBFOLDER + @"\FINRA";
        static private string ARCHIVE_FOLDER = DAILY_ARCHIVE_SUBFOLDER + @"\FINRA";

        // IStockDataProvider Implementation
        public override bool SupportsIntradayDownload
        {
            get { return false; }
        }
        public override bool LoadData(string rootFolder, StockSerie stockSerie)
        {
            if (stockSerie.StockName.StartsWith("PCR."))
            {
                if (stockSerie.StockName.Contains("VIX"))
                {
                    return this.ParseVIXPCRatioCSV(stockSerie, rootFolder + @"\\data\\daily\\FINRA\\" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv");
                }
                else
                {
                    return this.ParsePCRatioCSV(stockSerie, rootFolder + @"\\data\\daily\\FINRA\\" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv");
                }
            }
            else
            {
                return this.ParseFINRAIndexCSV(stockSerie, rootFolder + @"\\data\\daily\\FINRA\\" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".csv");
            }
        }
        public override void InitDictionary(string rootFolder, StockDictionary stockDictionary, bool download)
        {
            // Create data folder if not existing
            if (!Directory.Exists(rootFolder + FOLDER))
            {
                Directory.CreateDirectory(rootFolder + FOLDER);
            }
            string[] names = new string[] { "BBREADTH.ALL", "BBREADTH.HY", "BBREADTH.IG" };
            StockSerie stockSerie = null;
            foreach (string name in names)
            {
                if (!stockDictionary.ContainsKey(name))
                {
                    stockSerie = new StockSerie(name, name, StockSerie.Groups.INDICATOR, StockDataProvider.FINRA);
                    stockDictionary.Add(name, stockSerie);
                    if (download && this.needDownload)
                    {
                        //this.DownloadDailyData(rootFolder, stockSerie);
                    }
                }
            }
        }
        public override bool DownloadDailyData(string rootFolder, StockSerie stockSerie)
        {
            // http://finra-markets.morningstar.com/transferPage.jsp?path=http%3A%2F%2Fmuni-internal.morningstar.com%2Fpublic%2FMarketBreadth%2FC&date=11%2F14%2F2006&_=1480663564792
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                bool isUpTodate = false;
                stockSerie.Initialise();
                DateTime lastDate;
                if (stockSerie.Count > 0)
                {
                    // This serie already exist, download just the missing data.
                    lastDate = stockSerie.Keys.Last();
                }
                else
                {
                    lastDate = DateTime.MinValue;
                }

                isUpTodate = (lastDate >= DateTime.Today) ||
                    (lastDate.DayOfWeek == DayOfWeek.Friday && (DateTime.Now - lastDate).Days <= 3 && DateTime.UtcNow.Hour < 23) ||
                    (lastDate == DateTime.Today.AddDays(-1) && DateTime.UtcNow.Hour < 23);

                if (!isUpTodate)
                {
                    NotifyProgress("Downloading " + stockSerie.StockGroup.ToString() + " - " + stockSerie.StockName);
                    string fileName = stockSerie.StockName + "_" + stockSerie.StockGroup + ".csv";
                    if (stockSerie.StockName.StartsWith("PCR."))
                    {
                        this.DownloadFINRAPutCallRatioData(rootFolder + FOLDER, fileName);
                    }
                    else
                    {
                        this.DownloadFileFromFINRA(rootFolder + FOLDER, fileName, stockSerie.StockName);
                    }
                    if (stockSerie.StockName == "PCR.EQUITY") // Check if something new has been downloaded using PCR.EQUITY as the reference for all downloads
                    {
                        this.ParsePCRatioCSV(stockSerie, rootFolder + FOLDER + "\\" + fileName);
                        if (lastDate == stockSerie.Keys.Last())
                        {
                            this.needDownload = false;
                        }
                    }
                }
                else
                {
                    if (stockSerie.StockName == "PCR.EQUITY") // Check if something new has been downloaded using PCR.EQUITY as the reference for all downloads
                    {
                        this.needDownload = false;
                    }
                }
                stockSerie.IsInitialised = isUpTodate;
            }
            return true;
        }
        public bool DownloadFINRAMarketData(string destRootFolder, ref bool upToDate)
        {
            upToDate = false;
            bool success = true;

            string folder = destRootFolder + @"\" + DAILY_SUBFOLDER;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            // Parse FINRADownload.cfg file
            using (StreamReader sr = new StreamReader(StockAnalyzerSettings.Properties.Settings.Default.RootFolder + "\\FINRADownload.cfg", true))
            {
                sr.ReadLine(); // Skip first line
                while (success && !sr.EndOfStream)
                {
                    string[] row = sr.ReadLine().Split(',');

                    success = this.DownloadFileFromFINRA(folder, row[1] + "_" + row[2] + ".csv", row[0]);
                }
            }
            return success;
        }
        private bool DownloadFileFromFINRA(string destFolder, string fileName, string indexName)
        {
            string url = string.Empty;

            if (indexName.ToUpper().CompareTo("VXV") == 0)
            {
                url = "http://www.FINRA.com/publish/scheduledtask/mktdata/datahouse/vxvdailyprices.csv";
            }
            else
            {
                url = @"http://www.FINRA.com/publish/Scheduledtask/mktdata/datahouse/$INDEX_NAMEhistory.csv";

                // Build URL
                url = url.Replace("$INDEX_NAME", indexName);
            }
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    wc.DownloadFile(url, destFolder + "\\" + fileName);
                }
            }
            catch (SystemException e)
            {
                StockLog.Write(e);
                return false;
            }
            return true;
        }

        public bool DownloadFINRAPutCallRatioData(string destFolder, string file)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }

            string[] fileNames = new string[] { "equityPC.csv", "indexPC.csv", "totalPC.csv", "vixPC.csv" };
            string[] destFileNames = new string[] { "PCR.EQUITY_INDICATOR.csv", "PCR.INDEX_INDICATOR.csv", "PCR.TOTAL_INDICATOR.csv", "PCR.VIX_INDICATOR.csv" };
            int i = 0;
            foreach (string fileName in fileNames)
            {
                if (destFileNames[i] != file)
                {
                    i++;
                    continue;
                }
                string url = @"http://www.FINRA.com/publish/ScheduledTask/MktData/datahouse/" + fileName;
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Proxy.Credentials = CredentialCache.DefaultCredentials;
                        wc.DownloadFile(url, destFolder + "\\" + destFileNames[i]);
                    }
                    break;
                }
                catch (SystemException e)
                {
                    StockLog.Write(e);
                    return false;
                }
            }
            return true;
        }


        // Private methods
        private bool ParseVIXPCRatioCSV(StockSerie stockSerie, string fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    try
                    {
                        StockDailyValue readValue = null;

                        // Skip 2 first lines
                        string name = "PCR.VIX";
                        sr.ReadLine();
                        sr.ReadLine();
                        while (sr.ReadLine().Contains("\"") && !sr.EndOfStream) ;

                        while (!sr.EndOfStream)
                        {
                            readValue = null;
                            string[] row = sr.ReadLine().Split(',');
                            if (row.GetLength(0) == 5)
                            {
                                readValue = new StockDailyValue(
                                    name,
                                    float.Parse(row[1], usCulture),
                                    float.Parse(row[1], usCulture),
                                    float.Parse(row[1], usCulture),
                                    float.Parse(row[1], usCulture),
                                    long.Parse(row[4], usCulture),
                                    DateTime.Parse(row[0], usCulture));
                                stockSerie.Add(readValue.DATE, readValue);
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        StockLog.Write(e);
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool ParsePCRatioCSV(StockSerie stockSerie, string fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    StockDailyValue readValue = null;

                    while (!sr.ReadLine().EndsWith("P/C Ratio") && !sr.EndOfStream) ;

                    while (!sr.EndOfStream)
                    {
                        try
                        {
                            string[] row = sr.ReadLine().Split(',');
                            float ratio = float.Parse(row[4], usCulture);

                            //ratio = (float)Math.Log10(ratio);
                            //if (inverse) ratio = -ratio;

                            readValue = new StockDailyValue(
                                stockSerie.StockName,
                                ratio,
                                ratio,
                                ratio,
                                ratio,
                                long.Parse(row[3], usCulture),
                                DateTime.Parse(row[0], usCulture));
                            if (readValue != null && !stockSerie.ContainsKey(readValue.DATE))
                            {
                                stockSerie.Add(readValue.DATE, readValue);
                            }
                        }
                        catch (System.Exception e)
                        {
                            StockLog.Write(e);
                            return false;
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
        private bool ParseFINRAIndexCSV(StockSerie stockSerie, string fileName)
        {
            if (File.Exists(fileName))
            {
                using (StreamReader sr = new StreamReader(fileName))
                {
                    try
                    {
                        StockDailyValue readValue = null;

                        sr.ReadLine();  // Skip the first line
                        sr.ReadLine();  // Skip the second line

                        while (!sr.EndOfStream)
                        {
                            // File format
                            // Date,Close
                            // 10-May-07,27.09
                            string[] row = sr.ReadLine().Split(',');
                            if (row.GetLength(0) == 2 && row[1] != "")
                            {
                                readValue = new StockDailyValue(
                                    stockSerie.StockName,
                                    float.Parse(row[1], usCulture),
                                    float.Parse(row[1], usCulture),
                                    float.Parse(row[1], usCulture),
                                    float.Parse(row[1], usCulture),
                                    100,
                                    DateTime.Parse(row[0], usCulture));

                                if (readValue != null)
                                {
                                    stockSerie.Add(readValue.DATE, readValue);
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        StockLog.Write(e);
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
