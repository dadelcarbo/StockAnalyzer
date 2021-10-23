using Microsoft.Win32;
using StockAnalyzer;
using StockAnalyzerSettings.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Serialization;
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
            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.LoadSettings();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Setting":
                    LoadSettings();
                    break;
            }
        }

        private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            this.ViewModel.Calculate();

            this.Cursor = Cursors.Arrow;

            if (indicator1Col != null)
            {
                indicator1Col.Header = ViewModel.Indicator1.Split('(')[0];
                if (indicator1Col.Header.ToString() == "ROR")
                {
                    indicator1Col.DataFormatString = "P2";
                }
                else
                {
                    indicator1Col.DataFormatString = null;
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

        #region Filter Persistency
        public static List<FilterSetting> SaveColumnFilters(Telerik.Windows.Controls.GridView.GridViewDataControl grid)
        {
            var settings = new List<FilterSetting>();

            foreach (Telerik.Windows.Data.IFilterDescriptor filter in grid.FilterDescriptors)
            {
                Telerik.Windows.Controls.GridView.IColumnFilterDescriptor columnFilter = filter as Telerik.Windows.Controls.GridView.IColumnFilterDescriptor;
                if (columnFilter != null)
                {
                    FilterSetting setting = new FilterSetting();

                    setting.ColumnUniqueName = columnFilter.Column.UniqueName;

                    setting.SelectedDistinctValues.AddRange(columnFilter.DistinctFilter.DistinctValues);

                    if (columnFilter.FieldFilter.Filter1.IsActive)
                    {
                        setting.Filter1 = new FilterDescriptorProxy();
                        setting.Filter1.Operator = columnFilter.FieldFilter.Filter1.Operator;
                        setting.Filter1.Value = columnFilter.FieldFilter.Filter1.Value;
                        setting.Filter1.IsCaseSensitive = columnFilter.FieldFilter.Filter1.IsCaseSensitive;
                    }

                    setting.FieldFilterLogicalOperator = columnFilter.FieldFilter.LogicalOperator;

                    if (columnFilter.FieldFilter.Filter2.IsActive)
                    {
                        setting.Filter2 = new FilterDescriptorProxy();
                        setting.Filter2.Operator = columnFilter.FieldFilter.Filter2.Operator;
                        setting.Filter2.Value = columnFilter.FieldFilter.Filter2.Value;
                        setting.Filter2.IsCaseSensitive = columnFilter.FieldFilter.Filter2.IsCaseSensitive;
                    }
                    settings.Add(setting);
                }
            }

            return settings;
        }

        public static void LoadColumnFilters(Telerik.Windows.Controls.GridView.GridViewDataControl grid
            , IEnumerable<FilterSetting> savedSettings)
        {
            grid.FilterDescriptors.SuspendNotifications();

            foreach (FilterSetting setting in savedSettings)
            {
                Telerik.Windows.Controls.GridViewColumn column = grid.Columns[setting.ColumnUniqueName];

                Telerik.Windows.Controls.GridView.IColumnFilterDescriptor columnFilter = column.ColumnFilterDescriptor;

                foreach (object distinctValue in setting.SelectedDistinctValues)
                {
                    columnFilter.DistinctFilter.AddDistinctValue(distinctValue);
                }

                if (setting.Filter1 != null)
                {
                    columnFilter.FieldFilter.Filter1.Operator = setting.Filter1.Operator;
                    columnFilter.FieldFilter.Filter1.Value = setting.Filter1.Value;
                    columnFilter.FieldFilter.Filter1.IsCaseSensitive = setting.Filter1.IsCaseSensitive;
                }

                columnFilter.FieldFilter.LogicalOperator = setting.FieldFilterLogicalOperator;

                if (setting.Filter2 != null)
                {
                    columnFilter.FieldFilter.Filter2.Operator = setting.Filter2.Operator;
                    columnFilter.FieldFilter.Filter2.Value = setting.Filter2.Value;
                    columnFilter.FieldFilter.Filter2.IsCaseSensitive = setting.Filter2.IsCaseSensitive;
                }
            }

            grid.FilterDescriptors.ResumeNotifications();
        }
        #endregion
        private void clearFilters_Click(object sender, RoutedEventArgs e)
        {
            this.gridView.FilterDescriptors.Clear();
        }

        private void saveFilters_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = "ulc";
            saveFileDialog.Filter = "Ultimate Chartist Palmares settings (*.xml)|*.xml";
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.InitialDirectory = Path.Combine(Settings.Default.RootFolder, "Palmares");
            if (saveFileDialog.ShowDialog() != true)
                return;

            try
            {
                var filters = SaveColumnFilters(this.gridView);
                var palmaresSettings = new PalmaresSettings()
                {
                    FilterSettings = filters,
                    Group = this.ViewModel.Group,
                    BarDuration = this.ViewModel.BarDuration,
                    Indicator1 = this.ViewModel.Indicator1,
                    Indicator2 = this.ViewModel.Indicator2,
                    Indicator3 = this.ViewModel.Indicator3,
                    Stop = this.ViewModel.Stop
                };

                using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
                    settings.Indent = true;
                    System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(fs, settings);
                    XmlSerializer serializer = new XmlSerializer(typeof(PalmaresSettings));
                    serializer.Serialize(xmlWriter, palmaresSettings);
                }
                var name = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                if (!this.ViewModel.Settings.Contains(name))
                {
                    this.ViewModel.Settings.Insert(0, name);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "File saving Error !!!");
            }
        }
        private void LoadSettings()
        {
            string path = Path.Combine(StockAnalyzerSettings.Properties.Settings.Default.RootFolder, "Palmares");
            string fileName = Path.Combine(path, this.ViewModel.Setting + ".xml");
            if (File.Exists(fileName))
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open))
                {
                    System.Xml.XmlReaderSettings settings = new System.Xml.XmlReaderSettings();
                    settings.IgnoreWhitespace = true;
                    System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(fs, settings);
                    XmlSerializer serializer = new XmlSerializer(typeof(PalmaresSettings));
                    var palmaresSettings = (PalmaresSettings)serializer.Deserialize(xmlReader);

                    this.ViewModel.Group = palmaresSettings.Group;
                    this.ViewModel.BarDuration = palmaresSettings.BarDuration;
                    this.ViewModel.Indicator1 = palmaresSettings.Indicator1;
                    this.ViewModel.Indicator2 = palmaresSettings.Indicator2;
                    this.ViewModel.Indicator3 = palmaresSettings.Indicator3;
                    this.ViewModel.Stop = palmaresSettings.Stop;
                    LoadColumnFilters(this.gridView, palmaresSettings.FilterSettings);
                }
            }
        }
    }
}
