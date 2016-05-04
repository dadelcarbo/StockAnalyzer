using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace StockAnalyzerApp.CustomControl.AlertDialog
{
   /// <summary>
   /// Interaction logic for AlertControl.xaml
   /// </summary>
   public partial class AlertControl : UserControl
   {
      public AlertControl()
      {
         this.DataContext = StockAlert.ParseAlertFile();

         InitializeComponent();
      }
      
      public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;

      public ObservableCollection<StockAlert> Alerts { get; set; }
      

      private void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
      {
         this.DataContext = StockAlert.ParseAlertFile();
      }

      private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
      {
         // Open on the alert stock
         StockAlert alert = ((DataGrid) sender).SelectedItem as StockAlert;

         if (SelectedStockChanged != null) this.SelectedStockChanged(alert.StockName, alert.BarDuration, true);

         StockAnalyzerForm.MainFrame.SetThemeFromAlert(alert);
      }
   }
}
