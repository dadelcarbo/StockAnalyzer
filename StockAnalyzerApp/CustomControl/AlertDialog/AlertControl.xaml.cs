using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Telerik.Windows;
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
        public AlertControl()
        {
            InitializeComponent();

            this.grid.AddHandler(GridViewCellBase.CellDoubleClickEvent, new EventHandler<RadRoutedEventArgs>(OnCellDoubleClick), true);

            foreach (var item in StockAlertConfig.GetConfigs())
            {
                this.TimeFrameComboBox.Items.Add(item);
                if (this.selectedTimeFrame == null)
                {
                    this.SelectedTimeFrame = item;
                }
            }
        }
        private StockAlertConfig selectedTimeFrame;

        public StockAlertConfig SelectedTimeFrame
        {
            get
            {
                return selectedTimeFrame;
            }
            set
            {
                if (selectedTimeFrame != value)
                {
                    selectedTimeFrame = value;
                    this.TimeFrameComboBox.SelectedItem = selectedTimeFrame;
                }
            }
        }

        public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;

        private void ClearBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var alertLog = this.selectedTimeFrame.AlertLog;
            alertLog.Clear();
        }

        private void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (AlertDefs == null)
                //{
                //    Thread alertThread = new Thread(StockAnalyzerForm.MainFrame.GenerateIntradayAlert);
                //    alertThread.Name = "Intraday Alert";
                //    alertThread.Start();
                //}
                //else
                //{
                //}
                    Thread alertThread = new Thread(StockAnalyzerForm.MainFrame.GenerateAlert_Thread);
                    alertThread.Name = "Alert";
                    alertThread.Start(Tuple.Create(this.selectedTimeFrame.AlertDefs, this.selectedTimeFrame.AlertLog));
            }
            catch (Exception ex)
            {
                StockLog.Write(ex);
            }
        }

        private void OnCellDoubleClick(object sender, RadRoutedEventArgs e)
        {
            // Open on the alert stock
            StockAlert alert = ((RadGridView)sender).SelectedItem as StockAlert;

            if (alert == null) return;

            if (SelectedStockChanged != null) this.SelectedStockChanged(alert.StockName, alert.BarDuration, true);

            StockAnalyzerForm.MainFrame.SetThemeFromIndicator(alert.Indicator);

            StockAnalyzerForm.MainFrame.WindowState = FormWindowState.Normal;
        }
    }
}
