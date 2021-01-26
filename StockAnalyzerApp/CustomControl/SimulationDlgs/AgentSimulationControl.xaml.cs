using StockAnalyzer.StockAgent;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockDrawing;
using System;
using System.Drawing;
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
        private Form parent;
        public event StockAnalyzerForm.SelectedStockAndDurationAndIndexChangedEventHandler SelectedStockChanged;

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

            if (SelectedStockChanged != null)
            {
                try
                {
                    DrawingItem.CreatePersistent = false;
                    viewModel.Serie.StockAnalysis.DeleteTransientDrawings();

                    if (!viewModel.Serie.StockAnalysis.DrawingItems.ContainsKey(ViewModel.Duration))
                    {
                        viewModel.Serie.StockAnalysis.DrawingItems.Add(ViewModel.Duration, new StockDrawingItems());
                    }

                    int exitIndex = viewModel.IsClosed ? viewModel.ExitIndex : viewModel.Serie.LastIndex;
                    viewModel.Serie.StockAnalysis.DrawingItems[ViewModel.Duration].Add(
                        new Rectangle2D(
                            new PointF(viewModel.EntryIndex, viewModel.Serie.GetSerie(StockDataType.LOW)[viewModel.EntryIndex]),
                            new PointF(exitIndex, viewModel.Serie.GetSerie(StockDataType.HIGH)[exitIndex])));

                    DrawingItem.KeepTransient = true;
                    this.SelectedStockChanged(viewModel.Serie.StockName, Math.Max(0, viewModel.EntryIndex - 100), Math.Min(viewModel.Serie.LastIndex, exitIndex + 100), ViewModel.Duration, true);
                    this.parent.TopMost = true;
                    this.parent.TopMost = false;
                }
                catch { }
                finally
                {
                    DrawingItem.CreatePersistent = true;
                    DrawingItem.KeepTransient = false;
                }
            }
        }
    }
}
