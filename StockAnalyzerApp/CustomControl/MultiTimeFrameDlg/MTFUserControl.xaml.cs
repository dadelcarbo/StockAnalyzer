using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace StockAnalyzerApp.CustomControl.MultiTimeFrameDlg
{
   /// <summary>
   /// Interaction logic for MTFUserControl.xaml
   /// </summary>
   public partial class MTFUserControl : UserControl
   {
      public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;
      private MTFViewModel viewModel;

      public MTFUserControl()
      {
         viewModel = new MTFViewModel();
         this.DataContext = viewModel;

         InitializeComponent();
      }


      private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
      {
         DataGrid dataGrid = (sender as DataGrid);
         if (SelectedStockChanged == null || dataGrid.SelectedCells.Count <1) return;
         
         MTFViewModel.MTFTrend trend = dataGrid.SelectedCells[0].Item as MTFViewModel.MTFTrend;

         StockBarDuration duration = StockBarDuration.Daily;
         string headerName = dataGrid.SelectedCells[0].Column.Header.ToString();
         if (headerName != "Name" && dataGrid.SelectedCells.Count > 0)
            Enum.TryParse((dataGrid.SelectedCells[0].Column.Header as TextBlock).Text, out duration);

         this.SelectedStockChanged(trend.Name, duration, true);

         StockAnalyzerForm.MainFrame.SetThemeFromIndicator("TRAILSTOP|" + viewModel.IndicatorName);
      }
   }
}
