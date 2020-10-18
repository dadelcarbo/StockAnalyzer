using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.ConditionalStatisticsDlg
{
    /// <summary>
    /// Interaction logic for StatisticsControl.xaml
    /// </summary>
    public partial class ConditionalStatisticsControl : UserControl
    {
        CondStatisticsViewModel viewModel = new CondStatisticsViewModel("TRUE(3)", "HigherClose");

        public ConditionalStatisticsControl()
        {
            InitializeComponent();

            this.DataContext = viewModel;
        }

        private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            try
            {
                this.viewModel.CalculateCondProb();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.Cursor = Cursors.Arrow;
        }

        private void DataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName.StartsWith("P"))
            {
                (e.Column as DataGridTextColumn).Binding.StringFormat = "P2";
            }
        }
    }
}
