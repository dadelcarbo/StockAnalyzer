using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.ExpectedValueDlg
{
    /// <summary>
    /// Interaction logic for StatisticsControl.xaml
    /// </summary>
    public partial class StatisticsControl : UserControl
    {
        StatisticsViewModel viewModel = new StatisticsViewModel("TOPEMA(0,80,1)", "Bullish", 6);

        public StatisticsControl()
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;
            try
            {
                viewModel.Calculate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Arrow;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "ExpectedValue" || e.PropertyName == "MaxReturnValue" || e.PropertyName == "MinReturnValue")
            {
                (e.Column as DataGridTextColumn).Binding.StringFormat = "P2";
            }
            if (e.PropertyName == "ReturnValues")
            {
                e.Cancel = true;
            }
        }
    }
}
