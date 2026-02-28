using StockAnalyzer.StockAgent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace StockAnalyzerApp.CustomControl.SimulationDlgs
{
    /// <summary>
    /// Interaction logic for BackTestUserControl.xaml
    /// </summary>
    public partial class BackTestUserControl : System.Windows.Controls.UserControl
    {
        private readonly Form parent;
        public event StockAnalyzerForm.SelectedStockAndDurationAndIndexChangedEventHandler SelectedStockChanged;

        public BackTestViewModel ViewModel { get; set; }

        public BackTestUserControl(Form parentForm)
        {
            this.parent = parentForm;
            InitializeComponent();

            this.ViewModel = (BackTestViewModel)this.Resources["ViewModel"];
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
                    int exitIndex = viewModel.IsClosed ? viewModel.ExitIndex : viewModel.Serie.LastIndex;

                    this.SelectedStockChanged(viewModel.Serie.StockName, Math.Max(0, viewModel.EntryIndex - 100), Math.Min(viewModel.Serie.LastIndex, exitIndex + 100), ViewModel.BarDuration, true);

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
