using StockAnalyzer;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Documents.Spreadsheet.Model;

namespace StockAnalyzerApp.CustomControl.PalmaresDlg
{
    /// <summary>
    /// Interaction logic for PalmaresControl.xaml
    /// </summary>
    public partial class PalmaresControl : UserControl
    {
        private System.Windows.Forms.Form Form { get; }
        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;

        public PalmaresViewModel ViewModel;
        public PalmaresControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();
            this.Form = form;
            this.DataContext = this.ViewModel = this.Resources["ViewModel"] as PalmaresViewModel;
        }

        private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            this.ViewModel.Calculate();

            this.Cursor = Cursors.Arrow;

            if (indicator1Col!=null)
            {
                indicator1Col.Header = ViewModel.Indicator1.Split('(')[0];
                if (indicator1Col.Header.ToString() == "ROR")
                {
                    indicator1Col.DataFormatString = "P2";
                }
                else
                {
                    indicator1Col.DataFormatString =null;
                }
            }
            if (indicator2Col != null)
            {
                indicator2Col.Header = ViewModel.Indicator2.Split('(')[0];
                if (indicator2Col.Header.ToString() == "ROR")
                {
                    indicator2Col.DataFormatString = "P2";
                }
                else
                {
                    indicator2Col.DataFormatString = null;
                }
            }
            if (indicator3Col != null)
            {
                indicator3Col.Header = ViewModel.Indicator3.Split('(')[0];
                if (indicator3Col.Header.ToString() == "ROR")
                {
                    indicator3Col.DataFormatString = "P2";
                }
                else
                {
                    indicator3Col.DataFormatString = null;
                }
            }
        }

        GridViewDataColumn indicator1Col;
        GridViewDataColumn indicator2Col;
        GridViewDataColumn indicator3Col;

        private void RadGridView_AutoGeneratingColumn(object sender, Telerik.Windows.Controls.GridViewAutoGeneratingColumnEventArgs e)
        {
            var columnName = e.Column.Header.ToString();
            var col = e.Column as GridViewDataColumn;
            switch (columnName)
            {
                case "Variation":
                case "Stop":
                    col.DataFormatString = "P2";
                    break;
                case "Indicator1":
                    if (!string.IsNullOrEmpty(ViewModel.Indicator1))
                    {
                        indicator1Col = col;
                        col.Header = ViewModel.Indicator1.Split('(')[0];
                        if (col.Header.ToString() == "ROR")
                        {
                            col.DataFormatString = "P2";
                        }
                    }
                    break;
                case "Indicator2":
                    if (!string.IsNullOrEmpty(ViewModel.Indicator1))
                    {
                        indicator2Col = col;
                        col.Header = ViewModel.Indicator2.Split('(')[0];
                        if (col.Header.ToString() == "ROR")
                        {
                            col.DataFormatString = "P2";
                        }
                    }
                    break;
                case "Indicator3":
                    if (!string.IsNullOrEmpty(ViewModel.Indicator1))
                    {
                        indicator3Col = col;
                        col.Header = ViewModel.Indicator3.Split('(')[0];
                        if (col.Header.ToString() == "ROR")
                        {
                            col.DataFormatString = "P2";
                        }
                    }
                    break;
            }
        }
        private void RadGridView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            var line = ((RadGridView)sender).SelectedItem as PalmaresLine;

            if (line == null) return;

            if (SelectedStockChanged != null)
            {
                StockAnalyzerForm.MainFrame.Activate();
                this.SelectedStockChanged(line.Name, ViewModel.BarDuration, true);
                if (!string.IsNullOrEmpty(this.ViewModel.Stop))
                {
                    StockAnalyzerForm.MainFrame.SetThemeFromIndicator($"TRAILSTOP|{this.ViewModel.Stop}");
                }
                this.Form.TopMost = true;
                this.Form.TopMost = false;
            }
        }

        private void exportToExcel_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            try
            {
                var currentCulture = CultureInfo.CurrentCulture;
                var weekNo = currentCulture.Calendar.GetWeekOfYear(
                                ViewModel.ToDate,
                                currentCulture.DateTimeFormat.CalendarWeekRule,
                                currentCulture.DateTimeFormat.FirstDayOfWeek);

                string exportFile = Path.Combine(StockAnalyzerSettings.Properties.Settings.Default.RootFolder, 
                    $@"CommentReport\Palmares_{ViewModel.Group}_{ViewModel.ToDate.Year}_{weekNo}.xlsx");

                using (FileStream fileStream = new FileStream(exportFile, FileMode.Create, FileAccess.Write))
                {
                    var options = new GridViewDocumentExportOptions()
                    {
                        ShowColumnHeaders = true,
                        ExportDefaultStyles = true
                    };
                    this.gridView.ExportToXlsx(fileStream, options);
                }
                System.Diagnostics.Process.Start(exportFile);

                Clipboard.SetText("=HYPERLINK(\"https://www.abcbourse.com/graphes/eod/\"&C2&\"p\";D2)");
            }
            catch (System.Exception ex)
            {
                StockAnalyzerException.MessageBox(ex);
            }
            this.Cursor = Cursors.Arrow;
        }

        private void gridView_ElementExportingToDocument(object sender, GridViewElementExportingToDocumentEventArgs e)
        {
            if (e.Element == ExportElement.Cell)
            {
                var cellExportingArgs = e as GridViewCellExportingEventArgs;
                if (cellExportingArgs?.Value == null)
                    return;

                if (cellExportingArgs.Column.DataFormatString == "P2")
                {
                    var tryDouble = double.TryParse(cellExportingArgs.Value.ToString().Replace("%", ""), out var d);
                    if (tryDouble)
                    {
                        var parameters = cellExportingArgs.VisualParameters as GridViewDocumentVisualExportParameters;
                        parameters.Style = new CellSelectionStyle()
                        {
                            Format = new CellValueFormat("0.00%")
                        };

                        cellExportingArgs.Value = d / 100.0;
                        return;
                    }
                }
                else
                {
                    var tryInt = int.TryParse(cellExportingArgs.Value.ToString(), out var i);
                    if (tryInt)
                    {
                        var parameters = cellExportingArgs.VisualParameters as GridViewDocumentVisualExportParameters;
                        parameters.Style = new CellSelectionStyle()
                        {
                            Format = new CellValueFormat("0")
                        };

                        cellExportingArgs.Value = i;
                        return;
                    }
                    var tryDouble = double.TryParse(cellExportingArgs.Value.ToString(), out var d);
                    if (tryDouble)
                    {
                        var parameters = cellExportingArgs.VisualParameters as GridViewDocumentVisualExportParameters;
                        parameters.Style = new CellSelectionStyle()
                        {
                            Format = new CellValueFormat("0.00")
                        };

                        cellExportingArgs.Value = d;
                        return;
                    }
                }
            }
        }
    }
}
