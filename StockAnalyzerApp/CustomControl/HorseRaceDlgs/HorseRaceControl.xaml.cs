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

      private void Cell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
      {
         if (this.SelectedStockChanged != null && (sender as DataGrid).SelectedItem is StockPosition)
         {
            StockPosition position = (sender as DataGrid).SelectedItem as StockPosition;
            this.SelectedStockChanged(position.Name, true);
         }
      }
   }
}
