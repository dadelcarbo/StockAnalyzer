using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData.DataProviders.SaxoTurbos.ConfigDialog;
using System.Diagnostics;
using System.Windows.Forms;

namespace StockAnalyzer.StockData.DataProviders.SaxoTurbos
{
    public class SaxoTurboDataProvider : DataProviderBase
    {
        public override string DisplayName => "Saxo Turbos";
        public override BarDuration[] SupportedDurations => new BarDuration[] { BarDuration.H_1, BarDuration.H_2, BarDuration.H_3, BarDuration.H_4 };

        public override BarDuration DefaultDuration => BarDuration.H_1;

        public override DataProvider Provider => DataProvider.SaxoTurbo;

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
    }
}
