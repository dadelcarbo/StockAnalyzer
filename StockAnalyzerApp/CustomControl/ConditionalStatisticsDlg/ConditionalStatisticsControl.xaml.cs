using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StockAnalyzerApp.CustomControl.ConditionalStatisticsDlg
{
    /// <summary>
    /// Interaction logic for StatisticsControl.xaml
    /// </summary>
    public partial class ConditionalStatisticsControl : UserControl
    {
        readonly CondStatisticsViewModel viewModel = new CondStatisticsViewModel("PaintBar", "TRUE(3)", "HigherClose", "TrailStop", "TRAILHL(3)", "HigherLow");

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
