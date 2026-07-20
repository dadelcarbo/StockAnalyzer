using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData.DataProviders.SaxoTurbos.ConfigDialog;
using StockAnalyzerSettings;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace StockAnalyzer.StockData.DataProviders.SaxoTurbos
{
    public class SaxoTurboDataProvider : DataProviderBase
    {
        public override string DisplayName => "Saxo Turbos";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.H_1, BarDuration.H_2, BarDuration.H_3, BarDuration.H_4 };

        public override BarDuration DefaultDuration => BarDuration.H_1;

        public override DataProvider Provider => DataProvider.SaxoTurbo;

        static public string SaxoUnderlyingFile => Path.Combine(Folders.PersonalFolder, "SaxoUnderlyings.cfg");

        protected override void PreInitDictionary(bool download) => this.dataClient = new SaxoTurboDataClient();

        protected override void PostInitDictionary(bool download) { }

        protected override StockInstrument CreateInstrumentFromConfigLine(string line)
        {
            var row = line.Split(',');

            return new StockInstrument()
            {
                Id = row[1],
                Name = row[2],
                Isin = row[1],
                Symbol = string.Empty,
                Group = Groups.TURBO,
                Provider = DataProvider.SaxoTurbo,
                Market = Market.TURBO
            };
        }

        public override void OpenInDataProvider(StockInstrument stockInstrument)
        {
            Process.Start($"https://fr-be.structured-products.saxo/products/{stockInstrument.Isin}");
        }

        public override DialogResult ShowConfigDialog(object param)
        {
            var configDlg = new SaxoDataProviderDlg() { StartPosition = FormStartPosition.CenterScreen };
            if (param != null && param is long saxoId)
            {
                configDlg.ViewModel.Initialize(saxoId);
            }
            else
            {
                configDlg.ViewModel.Initialize(0);
            }
            return configDlg.ShowDialog();
        }

        public static void InitSaxoIds()
        {
            string line;
            if (File.Exists(SaxoUnderlyingFile))
            {
                using var sr = new StreamReader(SaxoUnderlyingFile, true);
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    if (line.StartsWith("$")) break;

                    var row = line.Split(',');
                    var instrumentId = row[2];
                    if (string.IsNullOrEmpty(instrumentId))
                        continue;
                    if (StockDictionary.Instruments.TryGetValue(instrumentId, out var instrument))
                    {
                        instrument.SaxoId = long.Parse(row[0]);
                    }
                }
            }
        }
    }
}
