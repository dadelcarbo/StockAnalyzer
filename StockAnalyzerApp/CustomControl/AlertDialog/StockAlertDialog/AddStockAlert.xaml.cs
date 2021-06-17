using StockAnalyzer.StockClasses;
using System;
using System.Windows;
using System.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    /// <summary>
    /// Interaction logic for AddStockAlert.xaml
    /// </summary>
    public partial class AddStockAlert : UserControl
    {
        AddStockAlertDlg ParentDlg;
        public AddStockAlert(AddStockAlertDlg parent)
        {
            InitializeComponent();
            this.ParentDlg = parent;
        }
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (AddStockAlertViewModel)this.DataContext;
            var alertDef = new StockAlertDef()
            {
                StockName = viewModel.StockName,
                BarDuration = viewModel.BarDuration,
                CreationDate = DateTime.Now
            };
            switch (this.tabControl.SelectedIndex)
            {
                case 0:
                    // Create StockAlert from Graph Indicators
                    var fields = viewModel.IndicatorName.Split('|');
                    alertDef.IndicatorType = fields[0];
                    alertDef.IndicatorName = fields[1];
                    alertDef.EventName = viewModel.Event;
                    break;
                case 1:
                    alertDef.PriceTrigger = viewModel.Price;
                    alertDef.TriggerBrokenUp = viewModel.BrokenUp;
                    break;
                case 2:
                    break;
            }
            var alertConfig = StockAlertConfig.GetConfig("UserDefined");
            alertConfig.AlertDefs.Add(alertDef);
            StockAlertConfig.SaveConfig("UserDefined");
            this.ParentDlg.Ok();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.ParentDlg.Cancel();
        }
    }
}
