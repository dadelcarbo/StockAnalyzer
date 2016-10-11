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
using StockAnalyzer.StockClasses;

namespace StockAnalyzerApp.CustomControl.StatisticsDlg
{
   /// <summary>
   /// Interaction logic for StatisticsControl.xaml
   /// </summary>
   public partial class StatisticsControl : UserControl
   {
      StatisticsViewModel viewModel = new StatisticsViewModel("TRAILHL(9)", "BrokenUp", 6);

      public StatisticsControl()
      {
         InitializeComponent();

         this.DataContext = viewModel;
      }

      private void CalculateBtn_OnClick(object sender, RoutedEventArgs e)
      {
         this.Cursor = Cursors.Wait;

         try
         {
            int S1 = 0, R1 = 0, R2 = 0, count = 0;
            float avgReturn = 0;
            viewModel.Results.Clear();
            viewModel.Summary.Clear();
            foreach (var serie in StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup(viewModel.Group) && s.Initialise()))
            {
                if (viewModel.CalculateFixedStopProfit(serie.StockName))
               {
                  Console.WriteLine(serie.StockName + " " + viewModel.ToString());
                  S1 += viewModel.S1Count;
                  R1 += viewModel.R1Count;
                  R2 += viewModel.R2Count;
                  avgReturn += viewModel.TotalReturn;
                  count++;
               }
            }
            avgReturn /= (float) count;
            viewModel.Summary.Add(new StatisticsResult() { Name = "All", R1Count = R1, R2Count = R2, S1Count = S1, TotalReturn = avgReturn });
            
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message);
         }
         this.Cursor = Cursors.Arrow;
      }

      private void DataGrid_OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
      {
         if (e.PropertyName == "TotalReturn")
         {
            (e.Column as DataGridTextColumn).Binding.StringFormat = "P2";
         }
         if (e.PropertyName == "WinRatio")
         {
             (e.Column as DataGridTextColumn).Binding.StringFormat = "P2";
         }
      }
   }
}
