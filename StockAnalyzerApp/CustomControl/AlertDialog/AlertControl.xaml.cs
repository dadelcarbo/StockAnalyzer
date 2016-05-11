using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Xml.Serialization;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;

namespace StockAnalyzerApp.CustomControl.AlertDialog
{
   /// <summary>
   /// Interaction logic for AlertControl.xaml
   /// </summary>
   public partial class AlertControl : UserControl
   {
      public AlertControl()
      {
         InitializeComponent();
      }

      public event StockAnalyzerForm.SelectedStockAndDurationChangedEventHandler SelectedStockChanged;

      private void RefreshBtn_OnClick(object sender, RoutedEventArgs e)
      {
         try
         {
            Thread alertThread = new Thread(StockAnalyzerForm.MainFrame.GenerateAlert);
            alertThread.Start();
         }
         catch (Exception ex)
         {
            StockLog.Write(ex);
         }
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
