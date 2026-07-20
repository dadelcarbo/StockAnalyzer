using Saxo.OpenAPI.TradingServices;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockData.DataProviders;
using StockAnalyzer.StockData;
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
    /// Interaction logic for InstrumentControl.xaml
    /// </summary>
    public partial class InstrumentsControl : UserControl
    {
        private System.Windows.Forms.Form Form { get; }

        public event StockAnalyzerForm.SelectedInstrumentChangedEventHandler SelectedInstrumentChanged;

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
            var line = ((RadGridView)sender).SelectedItem as LineViewModel;

            if (line == null) return;

            this.Form.TopMost = true;
            StockAnalyzerForm.MainFrame.Activate();
            this.SelectedInstrumentChanged(line.Instrument, true);

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
                foreach (var instrument in this.gridView.Items.Cast<StockInstrument>())
                {
                    StockSplashScreen.ProgressText = "Downloading " + instrument.Group + " - " + instrument.DisplayName;

                    // DataProviderBase.ForceDownloadData(instrument);
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
                var instrumentGroups = this.gridView.Items.Cast<LineViewModel>().Select(s => s.Instrument).GroupBy(i => i.Provider);
                if (instrumentGroups.Any())
                {
                    foreach (var instruments in instrumentGroups)
                    {
                        var dp = DataProviderBase.GetDataProvider(instruments.Key);
                        dp.Remove(instruments);
                    }
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
                if (e.AddedCells[0].Item is SaxoUnderlyingViewModel saxoUnderlying)
                {
                    var serieName = string.IsNullOrEmpty(saxoUnderlying.InstrumentId) ? saxoUnderlying.SaxoName : saxoUnderlying.InstrumentId;
                    if (StockDictionary.Instruments.TryGetValue(serieName, out var instrument))
                    {
                        this.Form.TopMost = true;
                        StockAnalyzerForm.MainFrame.Activate();
                        this.SelectedInstrumentChanged(instrument, true);

                        this.Form.TopMost = false;
                    }
                    else
                    {
                        var previousInstrument = MainFrameViewModel.Instance.Instrument;
                        this.Form.TopMost = true;
                        StockAnalyzerForm.MainFrame.searchCombo.Text = saxoUnderlying.SaxoName.Split(' ')[0].Replace('/','-');
                        StockAnalyzerForm.MainFrame.searchCombo.Focus();

                        if (MainFrameViewModel.Instance.Instrument != previousInstrument)
                        {
                            saxoUnderlying.InstrumentId = MainFrameViewModel.Instance.Instrument.Id;
                        }


                        this.Form.TopMost = false;
                    }
                }
                else if (e.AddedCells[0].Item is SaxoInstrument)
                {
                    var saxoInstrument = e.AddedCells[0].Item as SaxoInstrument;
                    if (string.IsNullOrEmpty(saxoInstrument.Isin))
                        return;

                    var instrument = StockDictionary.Instruments.Values.FirstOrDefault(s => s.Isin == saxoInstrument.Isin);
                    if (instrument == null)
                        return;

                    this.Form.TopMost = true;
                    StockAnalyzerForm.MainFrame.Activate();
                    this.SelectedInstrumentChanged(instrument, true);

                    this.Form.TopMost = false;
                }
            }
        }
    }
}
