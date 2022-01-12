using StockAnalyzer.StockClasses;
using StockAnalyzerSettings;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.AlertDialog.StockAlertDialog
{
    /// <summary>
    /// Interaction logic for AddStockAlert.xaml
    /// </summary>
    public partial class AddStockAlert : UserControl
    {
        AddStockAlertViewModel ViewModel;
        AddStockAlertDlg ParentDlg;
        public AddStockAlert(AddStockAlertDlg parent, AddStockAlertViewModel viewModel)
        {
            this.ParentDlg = parent;
            this.ViewModel = viewModel;
            InitializeComponent();
            this.DataContext = viewModel;
        }
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            StockAlertConfig.SaveConfig();
            var fileName = Folders.Report + @"\LastGeneration.txt";
            if (File.Exists(fileName))
                File.Delete(fileName);
            this.ParentDlg.Ok();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            StockAlertConfig.ReloadConfig();
            this.ParentDlg.Cancel();
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ViewModel == null) return;

            var tabItem = this.tabControl.SelectedItem as TabItem;
            if (tabItem == null) return;

            this.ViewModel.AlertType = (AlertType)tabItem.Tag;
        }

        private void RadGridView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            var gridView = sender as RadGridView;
            var alertDef = gridView.SelectedItem as StockAlertDef;

            this.ViewModel.Init(alertDef);
        }
    }
}
