using StockAnalyzer.StockAgent;
using System;
using System.Windows;
using System.Windows.Forms;
using Telerik.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    /// <summary>
    /// Interaction logic for PortfolioSimulationControl.xaml
    /// </summary>
    public partial class PortfolioSimulationControl : System.Windows.Controls.UserControl
    {
        private Form parent;
        public event StockAnalyzerForm.SelectedStockAndDurationAndIndexChangedEventHandler SelectedStockChanged;

        public PortfolioSimulationViewModel ViewModel { get; set; }

        public PortfolioSimulationControl(Form parentForm)
        {
            this.parent = parentForm;
            InitializeComponent();

            this.ViewModel = (PortfolioSimulationViewModel)this.Resources["ViewModel"];
            this.ViewModel.SimulationCompleted += ViewModel_SimulationCompleted;
        }

        private void ViewModel_SimulationCompleted()
        {
            this.Cursor = System.Windows.Input.Cursors.Arrow;
        }

        private void performBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = System.Windows.Input.Cursors.Wait;
            this.ViewModel.Perform();
        }

        private void grid_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            var viewModel = ((RadGridView)sender).SelectedItem as StockTrade;

            if (viewModel == null) return;

            if (SelectedStockChanged != null)
            {
                try
                {
                    int exitIndex = viewModel.IsClosed ? viewModel.ExitIndex : viewModel.Serie.LastIndex;

                    this.SelectedStockChanged(viewModel.Serie.StockName, Math.Max(0, viewModel.EntryIndex - 100), Math.Min(viewModel.Serie.LastIndex, exitIndex + 100), ViewModel.Duration, true);

                    if (!string.IsNullOrEmpty(this.ViewModel?.DisplayIndicator))
                    {
                        StockAnalyzerForm.MainFrame.SetThemeFromIndicator(this.ViewModel?.DisplayIndicator);
                    }

                    this.parent.TopMost = true;
                    this.parent.TopMost = false;
                }
                catch { }
            }
        }
    }
}
