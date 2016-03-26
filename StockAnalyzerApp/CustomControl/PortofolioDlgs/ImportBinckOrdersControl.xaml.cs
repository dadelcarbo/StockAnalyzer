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

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs
{
   /// <summary>
   /// Interaction logic for ImportBinckOrdersControl.xaml
   /// </summary>
   public partial class ImportBinckOrdersControl : UserControl
   {

      public ImportBinckOrderViewModel ViewModel { get; set; }

      public ImportBinckOrdersControl()
      {
         this.ViewModel = new ImportBinckOrderViewModel();

         InitializeComponent();
      }

      private void Button_Import_Click(object sender, RoutedEventArgs e)
      {
         Cursor cursor = this.Cursor;
         this.Cursor = Cursors.Wait;
         this.ViewModel.Import();
         this.Cursor = cursor;
      }

      private void Button_Close_Click(object sender, RoutedEventArgs e)
      {
         this.ParentDialog.DialogResult = System.Windows.Forms.DialogResult.OK;
         this.ParentDialog.Close();
      }

      public ImportBinckOrderDlg ParentDialog { get; set; }
   }
}
