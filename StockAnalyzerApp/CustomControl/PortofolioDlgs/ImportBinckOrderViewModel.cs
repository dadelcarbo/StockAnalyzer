using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using StockAnalyzer.Portofolio;

namespace StockAnalyzerApp.CustomControl.PortofolioDlgs
{
   public class ImportBinckOrderViewModel : NotifyPropertyChanged
   {
      public IEnumerable<string> Portfolios { get; set; }

      private string selectedPortfolio;
      public string SelectedPortfolio
      {
         get { return selectedPortfolio; }
         set
         {
            if (selectedPortfolio != value)
            {
               selectedPortfolio = value;
               OnPropertyChanged("SelectedPortfolio");
            }
         }
      }

      private ObservableCollection<StockOrder> orders;
      public ObservableCollection<StockOrder> Orders
      {
         get { return orders; }
         set
         {
            if (orders != value)
            {
               orders = value;
               OnPropertyChanged("Orders");
            }
         }
      }

      public ImportBinckOrderViewModel()
      {
         this.Portfolios = StockAnalyzerForm.MainFrame.StockPortofolioList.Select(p => p.Name);
         this.selectedPortfolio = this.Portfolios.First();

         StockPortofolio portfolio = StockAnalyzerForm.MainFrame.StockPortofolioList.First(p => p.Name == selectedPortfolio);
         this.Orders = new ObservableCollection<StockOrder>(portfolio.OrderList);
      }

   }
}
