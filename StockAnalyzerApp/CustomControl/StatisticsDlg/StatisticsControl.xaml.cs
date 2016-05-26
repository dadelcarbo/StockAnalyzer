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

      private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
      {
         int S1 = 0, R1 = 0, R2 = 0;
         float Return = 0;
         viewModel.Results.Clear();
         foreach (var serie in StockDictionary.StockDictionarySingleton.Values.Where(s => s.BelongsToGroup("SRD")))
         {
            viewModel.Calculate(serie.StockName);

            Console.WriteLine(serie.StockName + " " + viewModel.ToString());
            S1 += viewModel.S1Count;
            R1 += viewModel.R1Count;
            R2 += viewModel.R2Count;
            Return += viewModel.TotalReturn;
         }
         viewModel.Results.Add(new StatisticsResult() { Name = "All", R1Count = R1, R2Count = R2, S1Count = S1, TotalReturn = Return});
      }
   }
}
