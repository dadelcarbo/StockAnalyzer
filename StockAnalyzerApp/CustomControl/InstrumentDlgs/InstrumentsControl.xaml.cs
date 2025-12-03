using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzer.StockClasses.StockDataProviders.AbcDataProvider;
using StockAnalyzer.StockClasses.StockDataProviders.StockDataProviderDlgs.SaxoDataProviderDialog;
using StockAnalyzerSettings;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    /// <summary>
    /// Interaction logic for InstrumentControl.xaml(
    /// </summary>
    public partial class InstrumentsControl : UserControl
    {
        private System.Windows.Forms.Form Form { get; }

        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        public InstrumentViewModel ViewModel;
        public InstrumentsControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();
            this.Form = form;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.DataContext = this.ViewModel = new InstrumentViewModel();
            }
        }

        private async void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            await this.ViewModel.CalculateAsync();

            this.Cursor = Cursors.Arrow;
        }
        private void RadGridView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            var line = ((RadGridView)sender).SelectedItem as StockSerie;

            if (line == null) return;

            this.Form.TopMost = true;
            StockAnalyzerForm.MainFrame.Activate();
            this.SelectedStockChanged(line.StockName, true);

            this.Form.TopMost = false;
        }

        private void Export_Onclick(object sender, RoutedEventArgs e)
        {
            Cursor cursor = this.Cursor;
            this.Cursor = Cursors.Wait;
            try
            {
                string exportFile = Path.Combine(Folders.PersonalFolder, $@"Instrument.xlsx");

                using (FileStream fileStream = new FileStream(exportFile, FileMode.Create, FileAccess.Write))
                {
                    var options = new GridViewDocumentExportOptions()
                    {
                        ShowColumnHeaders = true,
                        ExportDefaultStyles = true
                    };
                    this.gridView.ExportToXlsx(fileStream, options);
                }
                Process.Start(Folders.PersonalFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Excel Generation Error", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Cursor = cursor;
        }

        private void ForceDownloadBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            StockSplashScreen.FadeInOutSpeed = 0.25;
            StockSplashScreen.ProgressVal = 0;
            StockSplashScreen.ProgressMax = 100;
            StockSplashScreen.ProgressMin = 0;
            StockSplashScreen.ShowSplashScreen();

            try
            {
                foreach (var serie in this.gridView.Items.Cast<StockSerie>())
                {
                    StockSplashScreen.ProgressText = "Downloading " + serie.StockGroup + " - " + serie.StockName;

                    StockDataProviderBase.ForceDownloadSerieData(serie);
                }
            }
            catch { }

            StockSplashScreen.CloseForm(true);
            this.Cursor = Cursors.Arrow;

        }
        private void ExcludeSelectedBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            StockSplashScreen.FadeInOutSpeed = 0.25;
            StockSplashScreen.ProgressVal = 0;
            StockSplashScreen.ProgressMax = 100;
            StockSplashScreen.ProgressMin = 0;
            StockSplashScreen.ShowSplashScreen();

            try
            {
                var abcSeries = this.gridView.Items.Cast<StockSerie>().Where(s => s.DataProvider == StockDataProvider.ABC);
                if (abcSeries.Any())
                {
                    ABCDataProvider.AddToExcludedList(abcSeries.Select(s => s.ISIN));
                    foreach (var serie in abcSeries)
                    {
                        serie.StockAnalysis.Excluded = true;
                    }
                }

                foreach (var serie in this.gridView.Items.Cast<StockSerie>().Where(s => s.DataProvider != StockDataProvider.ABC))
                {
                    var dp = StockDataProviderBase.GetDataProvider(serie.DataProvider);
                    var handled = dp.RemoveEntry(serie);

                    serie.StockAnalysis.Excluded = true;
                }

            }
            catch { }

            StockSplashScreen.CloseForm(true);
            this.Cursor = Cursors.Arrow;
        }

        private void RadGridView_SelectedCellsChanged(object sender, Telerik.Windows.Controls.GridView.GridViewSelectedCellsChangedEventArgs e)
        {
            if (e.AddedCells.Count > 0)
            {
                if (e.AddedCells[0].Item is SaxoUnderlying)
                {
                    var item = e.AddedCells[0].Item as SaxoUnderlying;

                    var serieName = string.IsNullOrEmpty(item.SerieName) ? item.SaxoName : item.SerieName;
                    if (StockDictionary.Instance.ContainsKey(serieName))
                    {
                        this.Form.TopMost = true;
                        StockAnalyzerForm.MainFrame.Activate();
                        this.SelectedStockChanged(serieName, true);

                        this.Form.TopMost = false;
                    }
                    else
                    {
                        this.Form.TopMost = true;
                        StockAnalyzerForm.MainFrame.searchCombo.Text = serieName;
                        StockAnalyzerForm.MainFrame.searchCombo.Focus();

                        this.Form.TopMost = false;
                    }
                }
                else if (e.AddedCells[0].Item is Instrument)
                {
                    var item = e.AddedCells[0].Item as Instrument;
                    if (string.IsNullOrEmpty(item.Isin))
                        return;

                    var stockSerie = StockDictionary.Instance.Values.FirstOrDefault(s => s.ISIN == item.Isin);
                    if (stockSerie == null)
                        return;

                    this.Form.TopMost = true;
                    StockAnalyzerForm.MainFrame.Activate();
                    this.SelectedStockChanged(stockSerie.StockName, true);

                    this.Form.TopMost = false;
                }
            }
        }
    }
}
