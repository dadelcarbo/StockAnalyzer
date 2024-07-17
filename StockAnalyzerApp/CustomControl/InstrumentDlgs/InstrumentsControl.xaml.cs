using Microsoft.Win32;
using StockAnalyzer;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace StockAnalyzerApp.CustomControl.InstrumentDlgs
{
    /// <summary>
    /// Interaction logic for InstrumentControl.xaml
    /// </summary>
    public partial class InstrumentsControl : UserControl
    {
        private System.Windows.Forms.Form Form { get; }

        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;

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

            StockAnalyzerForm.MainFrame.Activate();
            this.SelectedStockChanged(line.StockName, BarDuration.Daily, true);

            this.Form.TopMost = true;
            this.Form.TopMost = false;
        }

    }
}
