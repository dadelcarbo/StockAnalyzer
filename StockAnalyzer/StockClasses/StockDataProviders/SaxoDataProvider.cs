using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockData;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StockAnalyzer.StockClasses.StockDataProviders
{
    public class SaxoDataProvider : StockDataProviderBase
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
        }

        static readonly ChartService chartService = new ChartService();

        public override bool DownloadDailyData(StockSerie stockSerie)
        {
            int horizon;
            switch (stockSerie.BarDuration)
            {
                case BarDuration.Daily:
                    horizon = 1440;
                    break;
                case BarDuration.Weekly:
                    horizon = 10080;
                    break;
                case BarDuration.Monthly:
                    horizon = 43200;
                    break;
                case BarDuration.M_5:
                    horizon = 5;
                    break;
                case BarDuration.M_10:
                    horizon = 10;
                    break;
                case BarDuration.M_15:
                    horizon = 15;
                    break;
                case BarDuration.M_30:
                    horizon = 30;
                    break;
                case BarDuration.H_1:
                    horizon = 60;
                    break;
                case BarDuration.H_2:
                    horizon = 120;
                    break;
                case BarDuration.H_4:
                    horizon = 240;
                    break;
                default:
                    throw new StockAnalyzerException($"Duration: {stockSerie.BarDuration} is not supported in Saxo OpenAPI");
            }
            var bars = chartService.GetData(stockSerie.Uic, horizon);
            if (bars == null || bars.Length == 0)
                return false;
            stockSerie.IsInitialised = false;
            foreach (var bar in bars.Select(ohlc => new StockDailyValue(ohlc.Open, ohlc.High, ohlc.Low, ohlc.Close, (long)ohlc.Volume, ohlc.Time)))
            {
                stockSerie.Add(bar.DATE, bar);
            }
            return true;
        }

        static readonly InstrumentService instrumentService = new InstrumentService();

        private void InitFromFile(StockDictionary stockDictionary, bool download, string fileName)
        {
            return;
            if (File.Exists(fileName))
            {
                using var sr = new StreamReader(fileName, true);
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    if (line.StartsWith("$")) break;

                    var row = line.Split(',');
                    var saxoInstrument = instrumentService.GetInstrumentById(long.Parse(row[0]));
                    if (saxoInstrument != null && !stockDictionary.ContainsKey(saxoInstrument.Description))
                    {
                        var stockSerie = new StockSerie(saxoInstrument.Description, saxoInstrument.Symbol, Groups.SAXO, StockDataProvider.Saxo, BarDuration.Daily);
                        stockSerie.ISIN = row[1];
                        stockDictionary.Add(saxoInstrument.Description, stockSerie);

                        stockSerie.Uic = saxoInstrument.Identifier;
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

        public override void OpenInDataProvider(StockInstrument stockInstrument)
        {
            //Process.Start($"https://fr-be.structured-products.saxo/products/{stockSerie.ISIN}");
        }
    }
}
