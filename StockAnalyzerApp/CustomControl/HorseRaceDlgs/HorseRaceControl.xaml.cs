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
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace StockAnalyzerApp.CustomControl.HorseRaceDlgs
{
    /// <summary>
    /// Interaction logic for HorseRaceControl.xaml
    /// </summary>
    public partial class HorseRaceControl : UserControl
    {
        public HorseRaceViewModel ViewModel { get; set; }

        public event StockAnalyzerForm.SelectedStockChangedEventHandler SelectedStockChanged;

        public HorseRaceControl()
        {
            InitializeComponent();

            this.ViewModel = new HorseRaceViewModel();
        }

        private void positionGridView_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangeEventArgs e)
        {
            var viewModel = this.positionGridView?.SelectedItem as StockPosition;
            if (viewModel == null) return;

            if (SelectedStockChanged != null)
            {
                this.SelectedStockChanged(viewModel.Name, true);
                StockAnalyzerForm.MainFrame.Activate();
            }
        }

        private void grid_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            var column = e.Column as Telerik.Windows.Controls.GridViewBoundColumnBase;
            if (column != null && column.DataType == typeof(string))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.Contains;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.Contains;
            }
            else if (column != null && column.DataType == typeof(DateTime))
            {
                e.DefaultOperator1 = Telerik.Windows.Data.FilterOperator.IsGreaterThanOrEqualTo;
                e.DefaultOperator2 = Telerik.Windows.Data.FilterOperator.IsGreaterThanOrEqualTo;
            }
        }
    }
}
