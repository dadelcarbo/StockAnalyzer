using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;
using StockAnalyzerSettings;
using System;
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
    /// Interaction logic for InstrumentControl.xaml
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
            this.DataContext = this.ViewModel = this.Resources["ViewModel"] as InstrumentViewModel;
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
                foreach (var serie in this.gridView.Items.Cast<StockSerie>())
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
    }
}
