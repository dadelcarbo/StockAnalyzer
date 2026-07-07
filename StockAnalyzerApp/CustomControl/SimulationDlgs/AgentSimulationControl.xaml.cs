using StockAnalyzer.StockAgent;
using System;
using System.Windows;
using System.Windows.Forms;
using Telerik.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    /// <summary>
    /// Interaction logic for AgentSimulationControl.xaml
    /// </summary>
    public partial class AgentSimulationControl : System.Windows.Controls.UserControl
    {
        private readonly Form parent;
        public event StockAnalyzerForm.SelectedInstrumentAndDurationAndIndexChangedEventHandler SelectedInstrumentChanged;

        public AgentSimulationViewModel ViewModel { get; set; }
        public AgentSimulationControl(Form parentForm)
        {
            this.parent = parentForm;
            InitializeComponent();

            this.ViewModel = (AgentSimulationViewModel)this.Resources["ViewModel"];
        }

        private void performBtn_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.Perform();
        }

        private void grid_SelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            // Open on the alert stock
            var viewModel = ((RadGridView)sender).SelectedItem as StockTrade;

            if (viewModel == null) return;

            if (SelectedInstrumentChanged != null)
            {
                try
                {
                    int exitIndex = viewModel.IsClosed ? viewModel.ExitIndex : viewModel.DataSerie.LastIndex;

                    this.SelectedInstrumentChanged(viewModel.Instrument, Math.Max(0, viewModel.EntryIndex - 100), Math.Min(viewModel.DataSerie.LastIndex, exitIndex + 100), ViewModel.BarDuration, true);

                    if (!string.IsNullOrEmpty(this.ViewModel.BestAgent?.DisplayIndicator))
                    {
                        StockAnalyzerForm.MainFrame.SetThemeFromIndicator(this.ViewModel.BestAgent?.DisplayIndicator);
                    }

                    this.parent.TopMost = true;
                    this.parent.TopMost = false;
                }
                catch { }
            }
        }
    }
}
