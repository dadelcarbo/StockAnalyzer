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
      public StockPortofolio(string name, float totalDeposit)
      {
         Currency = '€';
         Group = StockSerie.Groups.ALL;
         this.Name = name;
         this.OrderList = new StockOrderList();
         this.IsSimulation = false;
         this.IsVirtual = false;
         this.TotalDeposit = totalDeposit;
      }
      class PositionValues
      {
         public int Position { get; set; }
         public List<StockDailyValue> Values { get; set; }

         public float OpenValue { get; set; }
         public float MinValue { get; set; }
         public float MaxValue { get; set; }

         public PositionValues(int position, float openValue, List<StockDailyValue> values)
         {
            this.Position = position;
            this.Values = values;
            this.OpenValue = openValue;
            this.MinValue = float.MaxValue;
            this.MaxValue = float.MinValue;
         }

         public StockDailyValue AtDate(DateTime date)
         {
            return Values.FirstOrDefault(v => v.DATE == date);
         }

         public float MaxDrawdown
         {
            get
            {
               if (Position > 0)
                  return (MinValue - OpenValue) / OpenValue;
               else
                  return (OpenValue - MinValue) / OpenValue;
            }
         }
      }

      public StockSerie GeneratePortfolioStockSerie(string name, StockSerie referenceSerie, StockSerie.Groups group)
      {
         StockSerie stockSerie = new StockSerie(name, name, group, StockDataProvider.Portofolio);
         stockSerie.IsPortofolioSerie = true;

         float open = 0.0f;
         float high = 0.0f;
         float low = 0.0f;
         float close = 0.0f;
         int volume = 1;
         float cash = TotalDeposit;

         Dictionary<string, PositionValues> stockPositionDico = new Dictionary<string, PositionValues>();

          // Statistics

         int nbTrades = 0;
         int nbWinTrades = 0;
         float maxDrawdown = float.MaxValue;
         float maxGain = float.MinValue;
         float maxLoss = float.MinValue;
         float totalReturn = 0f;

         referenceSerie.Initialise();
         DateTime previousDate = referenceSerie.Keys.First();

         foreach (DateTime date in referenceSerie.GetValues(StockSerie.StockBarDuration.Daily).Select(v => v.DATE))
         {
            // Calculate open value
            
            // Retrieve orders for this date/time
            List<StockOrder> orderList = this.OrderList.FindAll(order => order.ExecutionDate == date);
            // @@@@ this.OrderList.FindAll(order => (order.ExecutionDate >= date.Date && order.ExecutionDate < date.Date.AddDays(1)));

            // Manage new orders
            foreach (StockOrder stockOrder in orderList)
            {
               int numberOfShare = stockOrder.IsShortOrder ? -stockOrder.Number : stockOrder.Number;
               if (stockOrder.IsBuyOrder()) // Opening position
               {
                  cash -= stockOrder.TotalCost;
                  if (stockPositionDico.ContainsKey(stockOrder.StockName))
                  {
                     stockPositionDico[stockOrder.StockName].Position += numberOfShare;
                     stockPositionDico[stockOrder.StockName].OpenValue = stockOrder.Value;
                  }
                  else
                  {
                     stockPositionDico.Add(stockOrder.StockName, new PositionValues(numberOfShare, stockOrder.Value, stockDictionary[stockOrder.StockName].GetValues(StockSerie.StockBarDuration.Daily)));
                  }
               }
               else // Closing Position
               {
                  cash += stockOrder.TotalCost;
                  if (stockPositionDico.ContainsKey(stockOrder.StockName))
                  {
                     PositionValues position = stockPositionDico[stockOrder.StockName];
                     if ( position.Position == numberOfShare)
                     {
                        maxDrawdown = Math.Min(maxDrawdown, position.MaxDrawdown);
                        stockPositionDico.Remove(stockOrder.StockName);
                        nbTrades++;
                     }
                     else
                     {
                        position.Position-= numberOfShare;
                     }
                     if (stockOrder.IsShortOrder)
                     {
                        if (position.OpenValue > stockOrder.Value)
                        {
                           nbWinTrades++;
                           maxGain = Math.Max(maxGain, (position.OpenValue - stockOrder.Value) / position.OpenValue);
                        }
                        else
                        {
                           maxLoss = Math.Max(maxLoss, -(position.OpenValue - stockOrder.Value) / position.OpenValue);
                        }
                     }
                     else
                     {
                        if (position.OpenValue < stockOrder.Value)
                        {
                           nbWinTrades++;
                           maxGain = Math.Max(maxGain, -(position.OpenValue - stockOrder.Value) / position.OpenValue);
                        }
                        else
                        {
                           maxLoss = Math.Max(maxLoss, (position.OpenValue - stockOrder.Value) / position.OpenValue);
                        }
                     }
                  }
                  else
                  {
                     throw new System.Exception("Sell order found on non bought stock");
                     // @@@@ Need to have proper error manegement otherwise the applications crashes.
                     return referenceSerie;
                  }
               }
            }

            // Calculate new value after taking into account the orders.
            low = cash;
            high = cash;
            close = cash;
            open = cash;
            if (stockPositionDico.Count != 0)
            {
               foreach (PositionValues position in stockPositionDico.Values)
               {
                  StockDailyValue currentValue = position.AtDate(date);
                  if (currentValue == null)
                  {
                     currentValue = position.AtDate(previousDate);
                  }
                  close += currentValue.CLOSE*position.Position;
                  open += currentValue.OPEN*position.Position;
                  if (position.Position > 0)
                  {
                     position.MaxValue = Math.Max(position.MaxValue, currentValue.HIGH);
                     position.MinValue = Math.Min(position.MinValue, currentValue.LOW);

                     low += currentValue.LOW*position.Position;
                     high += currentValue.HIGH*position.Position;
                  }
                  else
                  {
                     // We are facing a short order, everything is reversed
                     low += currentValue.HIGH*position.Position;
                     high += currentValue.LOW*position.Position;

                     position.MaxValue = Math.Max(position.MaxValue, currentValue.LOW);
                     position.MinValue = Math.Min(position.MinValue, currentValue.HIGH);
                  }
               }
            }

            StockDailyValue dailyValue = new StockDailyValue(name, open, high, low, close, volume, date);
            stockSerie.Add(date, dailyValue);
            dailyValue.Serie = stockSerie;

            previousDate = date;
         }

         Console.WriteLine("Statistics for " + stockSerie.StockName);
         Console.WriteLine("NbTrades: " + nbTrades);
         Console.WriteLine("Win %: " + ((float)nbWinTrades/(float)nbTrades).ToString("P2"));
         Console.WriteLine("MaxDrowdown: " + maxDrawdown.ToString("P2"));
         Console.WriteLine("MaxGain: " + maxGain.ToString("P2"));
         Console.WriteLine("MaxLoss: " + maxLoss.ToString("P2"));
                 
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