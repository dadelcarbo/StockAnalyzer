using System;
using System.Collections.Generic;
using System.Linq;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockClasses.StockDataProviders;

namespace StockAnalyzer.Portofolio
{
   public class StockPortofolio
   {
      #region PUBLIC PROPERTIES
      public string Name { get; set; }
      public float TotalDeposit { get; set; }
      public float OffsetLiquidity { get; set; }
      public char Currency { get; set; }
      public StockSerie.Groups Group { get; set; }
      public float CurrentStocksValue { get { return currentStockValue; } }
      public float TotalPortofolioValue { get { return CurrentStocksValue + AvailableLiquitidity; } }
      public float AvailableLiquitidity { get { return TotalDeposit + OffsetLiquidity + totalIncome; } }
      public float TotalAddedValue { get { return (TotalPortofolioValue - TotalDeposit) / TotalDeposit; } }
      public bool IsSimulation { get; set; }
      public bool IsVirtual { get; set; }
      public StockOrderList OrderList { get; set; }
      #endregion PUBLIC PROPERTIES

      public StockPortofolio Clone()
      {
         return (StockPortofolio)this.MemberwiseClone();
      }

      private float totalIncome = 0.0f;
      private float currentStockValue = 0.0f;

      private StockDictionary stockDictionary;
      public void Initialize(StockDictionary stockDictionary)
      {
         this.stockDictionary = stockDictionary;

         totalIncome = 0.0f;
         currentStockValue = 0.0f;

         // Calculate current Stock Value
         Dictionary<string, int> nbActiveStock = this.OrderList.GetNbActiveStock();

         foreach (string stockName in nbActiveStock.Keys)
         {
            if (stockName != this.Name)
            {
               currentStockValue += this.stockDictionary[stockName].Values.Last().CLOSE * nbActiveStock[stockName];
            }
         }

         // Calculate historic portofolio value
         foreach (StockOrder stockOrder in OrderList)
         {
            if (stockOrder.State == StockOrder.OrderStatus.Executed)
            {
               if (stockOrder.IsBuyOrder())
               {
                  totalIncome -= stockOrder.TotalCost;
               }
               else
               {
                  totalIncome += stockOrder.TotalCost;
               }
            }
         }
      }

      public StockPortofolio()
      {
         Currency = '€';
         Group = StockSerie.Groups.NONE;
         this.IsSimulation = false;
         this.IsVirtual = false;
         this.OrderList = new StockOrderList();
      }
      public StockPortofolio(string name)
      {
         Currency = '€';
         Group = StockSerie.Groups.NONE;
         this.Name = name;
         this.OrderList = new StockOrderList();
         this.IsSimulation = true;
         this.IsVirtual = true;
      }
      public StockSerie GeneratePortfolioStockSerie(string name, StockSerie referenceSerie, StockSerie.Groups group)
      {
         StockSerie stockSerie = new StockSerie(name, name, group, StockDataProvider.Portofolio);
         stockSerie.IsPortofolioSerie = true;

         float open = 0.0f;
         float high = 0.0f;
         float high1 = 0.0f;
         float high2 = 0.0f;
         float low = 0.0f;
         float low1 = 0.0f;
         float low2 = 0.0f;
         float close = 0.0f;
         int volume = 1;
         float cash = TotalDeposit;

         Dictionary<string, int> stockCountDico = new Dictionary<string, int>();

         foreach (DateTime date in referenceSerie.GetValues(StockAnalyzer.StockClasses.StockSerie.StockBarDuration.Daily).Select(v => v.DATE))
         {
            // Calculate open value
            open = cash;
            low1 = cash;
            high1 = cash;
            foreach (KeyValuePair<string, int> pair in stockCountDico)
            {
               if (stockDictionary[pair.Key].ContainsKey(date))
               {
                  StockDailyValue currentValue = stockDictionary[pair.Key][date];

                  open += currentValue.OPEN * pair.Value;
                  if (pair.Value > 0)
                  {
                     low1 += currentValue.LOW * pair.Value;
                     high1 += currentValue.HIGH * pair.Value;
                  }
                  else
                  {
                     low1 += currentValue.HIGH * pair.Value;
                     high1 += currentValue.LOW * pair.Value;
                  }
               }
            }

            // Retrieve orders for this date/time

            List<StockOrder> orderList = this.OrderList.FindAll(order => order.ExecutionDate == date);
            // @@@@ this.OrderList.FindAll(order => (order.ExecutionDate >= date.Date && order.ExecutionDate < date.Date.AddDays(1)));

            // Manage new orders
            foreach (StockOrder stockOrder in orderList)
            {
               int numberOfShare = stockOrder.IsShortOrder ? -stockOrder.Number : stockOrder.Number;
               if (stockOrder.IsBuyOrder())
               {
                  cash -= stockOrder.TotalCost;
                  if (stockCountDico.ContainsKey(stockOrder.StockName))
                  {
                     stockCountDico[stockOrder.StockName] = stockCountDico[stockOrder.StockName] + numberOfShare;
                  }
                  else
                  {
                     stockCountDico.Add(stockOrder.StockName, numberOfShare);
                  }
               }
               else
               {
                  cash += stockOrder.TotalCost;
                  if (stockCountDico.ContainsKey(stockOrder.StockName))
                  {
                     if (stockCountDico[stockOrder.StockName] == numberOfShare)
                     {
                        stockCountDico.Remove(stockOrder.StockName);
                     }
                     else
                     {
                        stockCountDico[stockOrder.StockName] = stockCountDico[stockOrder.StockName] - numberOfShare;
                     }
                  }
                  else
                  {
                     //throw new System.Exception("Sell order found on non bought stock");
                     // @@@@ Need to have proper error manegement otherwise the applications crashes.
                     return referenceSerie;
                  }
               }
            }

            // Calculate new value after taking into account the orders.
            low2 = cash;
            high2 = cash;
            close = cash;
            foreach (KeyValuePair<string, int> pair in stockCountDico)
            {
               if (stockDictionary[pair.Key].ContainsKey(date))
               {
                  StockDailyValue currentValue = stockDictionary[pair.Key][date];

                  close += currentValue.CLOSE * pair.Value;
                  if (pair.Value > 0)
                  {
                     low2 += currentValue.LOW * pair.Value;
                     high2 += currentValue.HIGH * pair.Value;
                  }
                  else
                  {  // We are facing a short order, everything is reversed
                     low2 += currentValue.HIGH * pair.Value;
                     high2 += currentValue.LOW * pair.Value;
                  }
               }
            }

            // Get low and high
            low = Math.Min(low1, low2);
            high = Math.Max(high1, high2);

            StockDailyValue dailyValue = new StockDailyValue(name, open, high, low, close, volume, date);
            stockSerie.Add(date, dailyValue);
            dailyValue.Serie = stockSerie;
         }

         // Preinitialise the serie
         stockSerie.PreInitialise();
         stockSerie.IsInitialised = true;
         return stockSerie;
      }
      public List<string> GetStockNames()
      {
         List<string> stockNames = new List<string>();
         foreach (StockOrder order in this.OrderList)
         {
            if (!stockNames.Contains(order.StockName))
            {
               stockNames.Add(order.StockName);
            }
         }
         stockNames.Sort();
         return stockNames;
      }

      public override string ToString()
      {
         string display = "Name:" + Name +
             " TotalDeposit:" + TotalDeposit +
             " OffsetLiquidity:" + OffsetLiquidity +
             " CurrentStocksValue:" + CurrentStocksValue +
             " TotalPortofolioValue:" + TotalPortofolioValue +
             " AvailableLiquitidity:" + AvailableLiquitidity +
             " TotalAddedValue:" + TotalAddedValue +
             " NbOrder:" + this.OrderList.Count;
         foreach (StockOrder order in this.OrderList)
         {
            display += System.Environment.NewLine + "\t" + order.ToString();
         }
         return display;
      }

      public void Clear()
      {
         this.OrderList.Clear();
         this.currentStockValue = 0;
         this.totalIncome = 0;
      }
   }
}