using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using UserControl = System.Windows.Controls.UserControl;

namespace StockAnalyzerApp.CustomControl.AlertDialog
{
    /// <summary>
    /// Interaction logic for AlertControl.xaml
    /// </summary>
    public partial class AlertControl : UserControl
    {
        private System.Windows.Forms.Form Form { get; }
        public AlertControl(System.Windows.Forms.Form form)
        {
            InitializeComponent();

            this.Form = form;
        }
        public StockAlertConfig SelectedTimeFrame => TimeFrameComboBox.SelectedItem == null ? StockAlertConfig.AlertConfigs.First() : TimeFrameComboBox.SelectedItem as StockAlertConfig;

        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;
        public event StockAnalyzerForm.SelectedStockAndDurationAndThemeChangedEventHandler SelectedStockAndThemeChanged;

        private void ClearBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var alertLog = this.SelectedTimeFrame.AlertLog;
            alertLog.Clear();
        }

        private void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var alertThread = new Thread(StockAnalyzerForm.MainFrame.GenerateAlert_Thread);
                alertThread.Name = "Alert";
                alertThread.Start(this.SelectedTimeFrame);
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
        }

        private void grid_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            StockAlert alert = ((RadGridView)sender).SelectedItem as StockAlert;

            if (alert == null) return;

            if (SelectedStockChanged != null)
            {
                StockAnalyzerForm.MainFrame.Activate();
                if (!string.IsNullOrEmpty(alert.Theme))
                {
                    this.SelectedStockAndThemeChanged(alert.StockName, alert.BarDuration, alert.Theme, true);
                }
                else
                {
                    this.SelectedStockChanged(alert.StockName, alert.BarDuration, true);
                    StockAnalyzerForm.MainFrame.SetThemeFromIndicator(alert.Indicator);
                }
                this.Form.TopMost = true;
                this.Form.TopMost = false;
            }
        }
        private void grid_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            var column = e.Column as Telerik.Windows.Controls.GridViewBoundColumnBase;
            if (column != null && column.DataType == typeof(string))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.Contains;
            }
            else if (column != null && column.DataType == typeof(DateTime))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.IsGreaterThanOrEqualTo;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.IsGreaterThanOrEqualTo;
            }
        }
    }
}
