using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.IO;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class SaxoDataProvider : StockDataProviderBase, IConfigDialog
    {
        static private readonly string ARCHIVE_FOLDER = INTRADAY_ARCHIVE_SUBFOLDER + @"\Saxo";
        static private readonly string INTRADAY_FOLDER = INTRADAY_SUBFOLDER + @"\Saxo";
        static private readonly string CONFIG_FILE = "SaxoDownload.cfg";


        public override void InitDictionary(StockDictionary stockDictionary, bool download)
        {
            // Create data folder if not existing
            if (!Directory.Exists(DataFolder + ARCHIVE_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + ARCHIVE_FOLDER);
            }
            if (!Directory.Exists(DataFolder + INTRADAY_FOLDER))
            {
                Directory.CreateDirectory(DataFolder + INTRADAY_FOLDER);
            }

            // Parse SaxoDownload.cfg file
            this.needDownload = false;
            InitFromFile(stockDictionary, download, Path.Combine(Folders.PersonalFolder, CONFIG_FILE));
        }

        public override bool SupportsIntradayDownload => true;

        public override bool LoadData(StockSerie stockSerie)
        {
            return DownloadDailyData(stockSerie);
            //var archiveFileName = DataFolder + ARCHIVE_FOLDER + "\\" + stockSerie.Symbol.Replace(':', '_') + "_" + stockSerie.StockName + "_" + stockSerie.StockGroup.ToString() + ".txt";
            //if (File.Exists(archiveFileName))
            //{
            //  stockSerie.ReadFromCSVFile(archiveFileName);
            //}
            //return stockSerie.Count > 0;
        }


        static readonly ChartService chartService = new ChartService();

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            var bars = chartService.GetData(stockSerie.Uic, stockSerie.BarDuration);
            if (bars == null || bars.Length == 0)
                return false;
            stockSerie.IsInitialised = false;
            foreach (var bar in bars)
            {
                stockSerie.Add(bar.DATE, bar);
            }
            return true;
        }

        static readonly InstrumentService instrumentService = new InstrumentService();

        private void InitFromFile(StockDictionary stockDictionary, bool download, string fileName)
        {
            return;
            string line;
            if (File.Exists(fileName))
            {
                using var sr = new StreamReader(fileName, true);
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

                    var row = line.Split(',');
                    var instrument = instrumentService.GetInstrumentById(long.Parse(row[0]));
                    if (instrument != null && !stockDictionary.ContainsKey(instrument.Description))
                    {
                        var stockSerie = new StockSerie(instrument.Description, instrument.Symbol, StockSerie.Groups.SAXO, StockDataProvider.Saxo, BarDuration.Daily);
                        stockSerie.ISIN = row[1];
                        stockDictionary.Add(instrument.Description, stockSerie);

                        stockSerie.Uic = instrument.Identifier;
                    }
                    else
                    {
                        StockLog.Write("Saxo Intraday Entry: " + row[1] + " already in stockDictionary");
                    }
                }
            }
        }

        public DialogResult ShowDialog(StockDictionary stockDico)
        {
            //Process.Start(Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));

            //var configDlg = new SaxoDataProviderDlg(stockDico, Path.Combine(Folders.PersonalFolder, CONFIG_FILE_USER));
            //configDlg.ShowDialog();

            return DialogResult.OK;
        }

        public override string DisplayName => "Saxo";

        public override void OpenInDataProvider(StockSerie stockSerie)
        {
            //Process.Start($"https://fr-be.structured-products.saxo/products/{stockSerie.ISIN}");
        }
    }
}
