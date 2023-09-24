using Microsoft.Win32;
using StockAnalyzer;
using StockAnalyzer.StockLogging;
using StockAnalyzerSettings;
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
        public event StockAnalyzerForm.SelectedStockAndDurationAndThemeChangedEventHandler SelectedStockAndThemeChanged;

        public PalmaresViewModel ViewModel;
        public PalmaresControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();
            this.Form = form;
            this.DataContext = this.ViewModel = this.Resources["ViewModel"] as PalmaresViewModel;
            this.ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            this.Loaded += PalmaresControl_Loaded;
        }

        private void PalmaresControl_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void GenerateColumns()
        {
            if (gridView.Columns.Count == 0)
                return;
            var column = gridView.Columns["Indicator1"] as GridViewDataColumn;
            if (string.IsNullOrEmpty(ViewModel.Indicator1))
            {
                column.IsVisible = false;
            }
            else
            {
                column.IsVisible = true;
                column.Header = ViewModel.Indicator1.Split('(')[0];
                if (column.Header.ToString() == "ROR" || column.Header.ToString() == "ROD" || column.Header.ToString() == "ROC")
                {
                    column.DataFormatString = "P2";
                }
                else
                {
                    column.DataFormatString = null;
                }
            }
            column = gridView.Columns["Indicator2"] as GridViewDataColumn;
            if (string.IsNullOrEmpty(ViewModel.Indicator2))
            {
                column.IsVisible = false;
            }
            else
            {
                column.IsVisible = true;
                column.Header = ViewModel.Indicator2.Split('(')[0];
                if (column.Header.ToString() == "ROR" || column.Header.ToString() == "ROD" || column.Header.ToString() == "ROC")
                {
                    column.DataFormatString = "P2";
                }
                else
                {
                    column.DataFormatString = null;
                }
            }
            column = gridView.Columns["Indicator3"] as GridViewDataColumn;
            if (string.IsNullOrEmpty(ViewModel.Indicator3))
            {
                column.IsVisible = false;
            }
            else
            {
                column.IsVisible = true;
                column.Header = ViewModel.Indicator3.Split('(')[0];
                if (column.Header.ToString() == "ROR" || column.Header.ToString() == "ROD" || column.Header.ToString() == "ROC")
                {
                    column.DataFormatString = "P2";
                }
                else
                {
                    column.DataFormatString = null;
                }
            }
            column = gridView.Columns["Stop"] as GridViewDataColumn;
            if (string.IsNullOrEmpty(ViewModel.Stop))
            {
                column.IsVisible = false;
            }
            else
            {
                column.IsVisible = true;
                column.DataFormatString = "P2";
            }

            column = gridView.Columns["Volume"] as GridViewDataColumn;
            column.IsVisible = true;
            column.DataFormatString = "F2";

            column = gridView.Columns["PeriodVariation"] as GridViewDataColumn;
            column.IsVisible = true;
            column.DataFormatString = "P2";

            column = gridView.Columns["BarVariation"] as GridViewDataColumn;
            column.IsVisible = true;
            column.DataFormatString = "P2";
        }

        private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            GenerateColumns();

            this.ViewModel.Calculate();

            this.Cursor = Cursors.Arrow;
        }
        private void RadGridView_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            var line = ((RadGridView)sender).SelectedItem as PalmaresLine;

            if (line == null) return;

            StockAnalyzerForm.MainFrame.Activate();
            if (string.IsNullOrEmpty(this.ViewModel.Theme))
            {
                this.SelectedStockChanged(line.Name, ViewModel.BarDuration, true);
                if (!string.IsNullOrEmpty(this.ViewModel.Stop))
                {
                    StockAnalyzerForm.MainFrame.SetThemeFromIndicator($"TRAILSTOP|{this.ViewModel.Stop}");
                }
            }
            else
            {
                this.SelectedStockAndThemeChanged(line.Name, ViewModel.BarDuration, this.ViewModel.Theme, true);
            }
            this.Form.TopMost = true;
            this.Form.TopMost = false;
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

                string exportFile = Path.Combine(Folders.Palmares, $@"Palmares_{ViewModel.Group}_{ViewModel.ToDate.Year}_{weekNo}.xlsx");

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
            catch (Exception ex)
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
        public static List<FilterSetting> SaveColumnFilters(GridViewDataControl grid)
        {
            var settings = new List<FilterSetting>();

            foreach (Telerik.Windows.Data.IFilterDescriptor filter in grid.FilterDescriptors)
            {
                IColumnFilterDescriptor columnFilter = filter as IColumnFilterDescriptor;
                if (columnFilter != null)
                {
                    FilterSetting setting = new FilterSetting();

                    setting.ColumnUniqueName = columnFilter.Column.UniqueName;
                    setting.DisplayName = columnFilter.Column.Header.ToString();

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

        public void LoadColumnFilters(GridViewDataControl grid, IEnumerable<FilterSetting> savedSettings)
        {
            if (grid.Columns.Count == 0)
                return;
            grid.FilterDescriptors.SuspendNotifications();
            try
            {
                this.clearFilters_Click(null, null);
                foreach (FilterSetting setting in savedSettings)
                {
                    Telerik.Windows.Controls.GridViewColumn column = grid.Columns[setting.ColumnUniqueName];
                    column.IsVisible = true;
                    column.Header = setting.DisplayName;

                    IColumnFilterDescriptor columnFilter = column.ColumnFilterDescriptor;
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
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
            finally
            {
                grid.FilterDescriptors.ResumeNotifications();
            }
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
            saveFileDialog.InitialDirectory = Folders.Palmares;
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
                    Stop = this.ViewModel.Stop,
                    Theme = this.ViewModel.Theme
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
            string fileName = Path.Combine(Folders.Palmares, this.ViewModel.Setting + ".xml");
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
                    this.ViewModel.Theme = palmaresSettings.Theme;
                    this.GenerateColumns();
                    LoadColumnFilters(this.gridView, palmaresSettings.FilterSettings);
                }
            }
        }

    }
}
