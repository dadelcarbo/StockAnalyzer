using System;
using System.Xml.Serialization;
using StockAnalyzer.StockClasses;
using StockAnalyzer.StockLogging;

namespace StockAnalyzer.Portofolio
{
   public class StockOrder : ICloneable
   {
      public enum OrderType
      {
         BuyAtMarketOpen,
         BuyAtMarketClose,
         BuyTrailing,
         BuyAtLimit,
         BuyAtThreshold,
         SellAtMarketOpen,
         SellAtMarketClose,
         SellTrailing,
         SellAtLimit,
         SellAtThreshold,
      }
      public enum OrderStatus
      {
         Executed,
         PartiallyExecuted,
         Pending,
         Expired
      }

      public string StockName { get; set; }
      public OrderType Type { get; set; }
      public DateTime CreationDate { get; set; }
      public DateTime ExecutionDate { get; set; }
      public DateTime ExpiryDate { get; set; }
      public OrderStatus State { get; set; }
      public bool IsShortOrder { get; set; }

      #region ID Management
      static protected int nextID = 0;
      static protected int getNextID()
      {
         return StockOrder.nextID++;
      }
      private int id;
      public int ID
      {
         get
         { return id; }
         set
         {
            id = value;
            StockOrder.nextID = Math.Max(StockOrder.nextID, value + 1);
         }
      }
      #endregion

      #region Stock Order Contructors
      public static StockOrder CreateExecutedOrder(string stockName, OrderType type, bool isShortOrder, DateTime creationDate, DateTime executionDate, int number, float value, float fee)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.StockName = stockName;
         stockOrder.Type = type;
         stockOrder.CreationDate = creationDate;
         stockOrder.ExecutionDate = executionDate;
         stockOrder.Number = number;
         stockOrder.ExecutedNumber = number;
         stockOrder.Value = value;
         stockOrder.Fee = fee;
         stockOrder.State = OrderStatus.Executed;
         stockOrder.IsShortOrder = isShortOrder;
         return stockOrder;
      }
      public static StockOrder CreateExecutedOrder(int id, string stockName, OrderType type, bool isShortOrder, DateTime creationDate, DateTime executionDate, int number, float value, float fee)
      {
         StockOrder stockOrder = new StockOrder(id);
         stockOrder.StockName = stockName;
         stockOrder.Type = type;
         stockOrder.CreationDate = creationDate;
         stockOrder.ExecutionDate = executionDate;
         stockOrder.Number = number;
         stockOrder.ExecutedNumber = number;
         stockOrder.Value = value;
         stockOrder.Fee = fee;
         stockOrder.State = OrderStatus.Executed;
         stockOrder.IsShortOrder = isShortOrder;
         return stockOrder;
      }
      public static StockOrder CreateBuyTrailingStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, float amountToInvest, float benchmark, float gapInPoints, StockDailyValue dailyValue)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.BuyTrailing;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.AmountToInvest = amountToInvest;
         stockOrder.Benchmark = benchmark;
         stockOrder.GapInPoints = gapInPoints;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         return stockOrder;
      }
      public static StockOrder CreateSellTrailingStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, int number, float benchmark, float gapInPoints, StockDailyValue dailyValue)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.SellTrailing;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.Number = number;
         stockOrder.Benchmark = benchmark;
         stockOrder.GapInPoints = gapInPoints;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         return stockOrder;
      }
      public static StockOrder CreateBuyAtLimitStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, float amountToInvest, float limit, StockDailyValue dailyValue, bool isShortOrder)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.BuyAtLimit;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.AmountToInvest = amountToInvest;
         stockOrder.Limit = limit;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         stockOrder.IsShortOrder = isShortOrder;
         if (limit > dailyValue.CLOSE)
         {
            StockLog.Write("Invalid limit in CreateBuyAtLimitStockOrder");
         }
         return stockOrder;
      }
      public static StockOrder CreateSellAtLimitStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, int number, float limit, StockDailyValue dailyValue, bool isShortOrder)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.SellAtLimit;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.Number = number;
         stockOrder.Limit = limit;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         stockOrder.IsShortOrder = isShortOrder;
         if (limit < dailyValue.CLOSE)
         {
            StockLog.Write("Invalid limit in CreateSellAtLimitStockOrder");
         }
         return stockOrder;
      }
      public static StockOrder CreateBuyAtThresholdStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, float amountToInvest, float threshold, StockDailyValue dailyValue, bool isShortOrder)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.BuyAtThreshold;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.AmountToInvest = amountToInvest;
         stockOrder.Threshold = threshold;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         stockOrder.IsShortOrder = isShortOrder;
         if (threshold < dailyValue.CLOSE)
         {
            throw new Exception("Invalid threshold"); // OK Due to dodgy shrt sell management
         }
         return stockOrder;
      }
      public static StockOrder CreateSellAtThresholdStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, int number, float threshold, StockDailyValue dailyValue, bool isShortOrder)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.SellAtThreshold;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.Number = number;
         stockOrder.Threshold = threshold;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         stockOrder.IsShortOrder = isShortOrder;
         if (threshold > dailyValue.CLOSE) // OK Due to dodgy short sell management
         {
           // throw new Exception("Invalid threshold");
         }
         return stockOrder;
      }
      public static StockOrder CreateBuyAtMarketOpenStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, float amountToInvest, bool isShortOrder)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.BuyAtMarketOpen;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.AmountToInvest = amountToInvest;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         stockOrder.IsShortOrder = isShortOrder;
         return stockOrder;
      }
      public static StockOrder CreateBuyAtMarketCloseStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, float amountToInvest, bool isShortOrder)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.BuyAtMarketClose;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.AmountToInvest = amountToInvest;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         stockOrder.IsShortOrder = isShortOrder;
         return stockOrder;
      }
      public static StockOrder CreateSellAtMarketOpenStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, int number, bool isShortOrder)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.SellAtMarketOpen;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.Number = number;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         stockOrder.IsShortOrder = isShortOrder;
         return stockOrder;
      }
      public static StockOrder CreateSellAtMarketCloseStockOrder(string stockName, DateTime creationDate, DateTime expiryDate, int number, bool isShortOrder)
      {
         StockOrder stockOrder = new StockOrder();
         stockOrder.Type = OrderType.SellAtMarketClose;
         stockOrder.StockName = stockName;
         stockOrder.CreationDate = creationDate;
         stockOrder.Number = number;
         stockOrder.State = OrderStatus.Pending;
         stockOrder.ExpiryDate = expiryDate;
         stockOrder.IsShortOrder = isShortOrder;
         return stockOrder;
      }
      #endregion

      #region Order processing
      protected bool HasExpired(DateTime currentDate)
      {
         return this.ExpiryDate < currentDate;
      }
      #endregion

      static public Comparison<StockOrder> DateComparison
      {
         get
         {
            return new Comparison<StockOrder>(StockOrder.compareDate);
         }
      }
      private static int compareDate(StockOrder stockOrder1, StockOrder stockOrder2)
      {
         return stockOrder1.ExecutionDate.CompareTo(stockOrder2.ExecutionDate);
      }

      public int Number { get; set; }
      [XmlIgnore]
      public int ExecutedNumber { get; set; }
      public float Value { get; set; }
      public float Fee { get; set; }
      static public float FixedFee { get; set; }
      static public float TaxRate { get; set; }
      public float UnitCost
      {
         get
         {
            if (this.Number != 0)
            {
               return Math.Abs(this.TotalCost) / this.Number;
            }
            else
            {
               return 0.0f;
            }
         }
      }
      public float TotalCost
      {
         get
         {
            float totalCost = 0.0f;
            if (this.IsBuyOrder())
            {
               if (!this.IsShortOrder)
               {
                  totalCost = this.Number * this.Value + this.Fee;
               }
               else
               {
                  totalCost -= this.Number * this.Value - this.Fee;
               }
            }
            else
            {
               if (!this.IsShortOrder)
               {
                  totalCost = this.Number * this.Value - this.Fee;
               }
               else
               {
                  totalCost -= this.Number * this.Value + this.Fee;
               }
            }
            return totalCost;
         }
      }
      public float Benchmark { get; set; }
      public float GapInPoints { get; set; }
      public float Limit { get; set; }
      public float Threshold { get; set; }
      public float AmountToInvest { get; set; }

      [XmlIgnore]
      public float TargetLimit
      {
         get
         {
            if (this.Type == OrderType.BuyTrailing)
            {
               return Benchmark + GapInPoints;
            }
            if (this.Type == OrderType.SellTrailing)
            {
               return Benchmark - GapInPoints;
            }
            return 0.0f;
         }
      }
      [XmlIgnore]
      public string PortofolioName { get; set; }

      public StockOrder()
      {
         StockName = string.Empty;
         Type = OrderType.BuyAtMarketClose;
         ExecutionDate = DateTime.Today;
         Number = 0;
         Value = 0.0f;
         Fee = 0.0f;
         // default constuctor doesn't set en ID
         ID = getNextID();
         this.ExecutedNumber = 0;
      }
      public StockOrder(int id)
      {
         StockName = string.Empty;
         Type = OrderType.BuyAtMarketClose;
         ExecutionDate = DateTime.Today;
         Number = 0;
         Value = 0.0f;
         Fee = 0.0f;
         ID = id;
         this.ExecutedNumber = 0;
      }
      private StockOrder(OrderType type, string stockName, DateTime creationDate, int number, float benchmark, float gapInPoints)
      {
         StockName = stockName;
         Type = type;
         CreationDate = creationDate;
         Number = number;
         Benchmark = benchmark;
         GapInPoints = gapInPoints;
         ID = getNextID();
         State = OrderStatus.Pending;
         this.ExecutedNumber = 0;
      }
      /// <summary>
      /// Return a new order matching new position. THis method should be rewritten using StockPositions.
      /// </summary>
      /// <param name="stockOrder"></param>
      /// <returns></returns>
      public StockOrder Add(StockOrder stockOrder)
      {
         // #### Need to be reviewed due to the Buy at market OPEN or CLOSE difference.
         if (this.StockName == stockOrder.StockName)
         {
            int newNumber = 0;
            float newValue = 0;
            if (this.IsBuyOrder())
            {
               if (stockOrder.IsBuyOrder())
               {
                  // Add new order to current position
                  newNumber = this.Number + stockOrder.Number;
                  newValue = (this.Value * this.Number + stockOrder.Value * stockOrder.Number) / (float)newNumber;
                  return CreateExecutedOrder(this.StockName, this.Type, false, this.CreationDate, stockOrder.ExecutionDate, newNumber, newValue, this.Fee + stockOrder.Fee);
               }
               else // Sell order
               {
                  newNumber = this.Number - stockOrder.Number;
                  //if (newNumber == 0)
                  //{
                  //    // Everything is sold.
                  //    return null;
                  //}
                  //else
                  //{
                  // Keep same value as initial order when selling it partially.
                  newValue = this.Value;
                  return CreateExecutedOrder(this.StockName, this.Type, false, this.CreationDate, stockOrder.ExecutionDate, newNumber, newValue, this.Fee + stockOrder.Fee);
                  //}
               }
            }
            else // Sell order
            {
               if (stockOrder.IsBuyOrder())
               {
                  newNumber = stockOrder.Number - this.Number;
                  if (newNumber == 0)
                  {
                     return null;
                  }
                  else
                  {
                     // Keep same value as initial order when covering it partially.
                     newValue = (stockOrder.Value * stockOrder.Number - this.Value * this.Number) / (float)newNumber;
                     return CreateExecutedOrder(this.StockName, this.Type, false, this.CreationDate, stockOrder.ExecutionDate, newNumber, newValue, this.Fee + stockOrder.Fee);
                  }
               }
               else // Sell order
               {
                  // TODO test
                  newNumber = this.Number + stockOrder.Number;
                  newValue = (this.Value * this.Number + stockOrder.Value * stockOrder.Number) / (float)newNumber;
                  return CreateExecutedOrder(this.StockName, this.Type, false, this.CreationDate, stockOrder.ExecutionDate, newNumber, newValue, this.Fee + stockOrder.Fee);
               }
            }
         }
         else
         {
            throw new System.ArithmeticException("Cannot add orders for different stock name");
         }

      }
      public StockPosition AddToPosition(StockOrder stockOrder)
      {
         StockPosition stockPosition = null;
         if (this.StockName == stockOrder.StockName && !(this.IsShortOrder ^ stockOrder.IsShortOrder))
         {
            stockPosition = StockPositionBase.CreatePosition(this);
            stockPosition.Add(stockOrder);
         }
         else
         {
            throw new System.ArithmeticException("Cannot add orders for different stock name");
         }
         return stockPosition;
      }
      override public string ToString()
      {
         return " Stock: " + StockName + " Type: " + Type.ToString() + " Short: " + IsShortOrder.ToString() +
             " ExecutionDate : " + ExecutionDate.ToShortDateString() +
             " Number: " + Number +
             " Value : " + Value.ToString("0.###") +
             " Fee: " + Fee.ToString("0.###") +
             " UnitCost : " + UnitCost.ToString("0.###") +
             " TotalCost: " + TotalCost.ToString("0.###");
      }
      public bool IsBuyOrder()
      {
         return Type < OrderType.SellAtMarketOpen;
      }
      #region Order processing
      public void ProcessOrder(StockDailyValue dailyValue)
      {
         if (this.State == OrderStatus.Expired)
         {
            throw new Exception("We don't process expired orders in this house !!!");
         }
         if (this.State == OrderStatus.Executed)
         {
            StockLog.Write("This order has already been executed");
            return;
         }
         //if (HasExpired(dailyValue.DATE))
         //{
         //   this.State = OrderStatus.Expired;
         //   return;
         //}
         switch (this.Type)
         {
            case OrderType.BuyAtMarketOpen:
               ExecuteBuyOrder(dailyValue, dailyValue.OPEN);
               break;
            case OrderType.BuyAtMarketClose:
               ExecuteBuyOrder(dailyValue, dailyValue.CLOSE);
               break;
            case OrderType.BuyTrailing:
               ProcessBuyTrailingOrder(dailyValue);
               break;
            case OrderType.BuyAtLimit:
               if (dailyValue.OPEN < this.Limit)
               {
                  ExecuteBuyOrder(dailyValue, dailyValue.HIGH);
               }
               else if (dailyValue.LOW < this.Limit)  // Worst case buy at limit even if OPEN is lower
               {
                  ExecuteBuyOrder(dailyValue, this.Limit);
               }
               break;
            case OrderType.BuyAtThreshold:
               if (dailyValue.OPEN > this.Threshold)
               {
                  ExecuteBuyOrder(dailyValue, dailyValue.OPEN);
               }
               else if (dailyValue.HIGH >= this.Threshold)
               {
                  ExecuteBuyOrder(dailyValue, this.Threshold);
               }
               break;
            case OrderType.SellAtMarketOpen:
               ExecuteSellOrder(dailyValue, dailyValue.OPEN);
               break;
            case OrderType.SellAtMarketClose:
               ExecuteSellOrder(dailyValue, dailyValue.CLOSE);
               break;
            case OrderType.SellTrailing:
               ProcessSellTrailingOrder(dailyValue);
               break;
            case OrderType.SellAtLimit:
               if (dailyValue.OPEN > this.Limit)
               {
                  ExecuteSellOrder(dailyValue, dailyValue.LOW);
               }
               else if (dailyValue.HIGH > this.Limit) // Worst case could open higher than the limit
               {
                  ExecuteSellOrder(dailyValue, this.Limit);
               }
               break;
            case OrderType.SellAtThreshold:
               if (dailyValue.OPEN <= this.Threshold)
               {
                  ExecuteSellOrder(dailyValue, dailyValue.OPEN);
               }
               else
                  if (dailyValue.LOW <= this.Threshold)
                  {
                     ExecuteSellOrder(dailyValue, this.Threshold);
                  }
               break;
            default:
               break;
         }

         // Debug check
         if (this.State == OrderStatus.Executed)
         {
            if (this.Value < dailyValue.LOW || this.Value > dailyValue.HIGH)
            {
               throw new Exception("Order executed out of the daily value boundary, please review your order creation");
            }
         }
      }
      private void ExecuteSellOrder(StockDailyValue dailyValue, float sellValue)
      {
         this.Value = sellValue;
         Fee = CalculateFee(this.Number, this.Value);
         this.ExecutionDate = dailyValue.DATE;
         this.State = OrderStatus.Executed;
      }

      private void ExecuteBuyOrder(StockDailyValue dailyValue, float buyValue)
      {
         this.Value = buyValue;
         CalculateNumberAndFeeFromAmount();
         this.ExecutionDate = dailyValue.DATE;
         this.State = OrderStatus.Executed;
      }

      private static float CalculateFee(int number, float unitValue)
      {
         float totalFee;
         totalFee = FixedFee + TaxRate * unitValue * number;
         return totalFee;
      }
      private void CalculateNumberAndFeeFromAmount()
      {
         Number = (int)(AmountToInvest / Value);
         Fee = CalculateFee(Number, Value);
         while (AmountToInvest < (Number * Value + Fee))
         {
            Number--;
            Fee = CalculateFee(Number, Value);
         }
      }
      private void ProcessBuyTrailingOrder(StockDailyValue dailyValue)
      {

         // Deal with stock market open fix
         if (dailyValue.OPEN >= this.TargetLimit) // Buy at open open price. It reallity it could buy lower before the market opens during fix.
         {
            // Execute Order
            this.Value = dailyValue.OPEN;
            CalculateNumberAndFeeFromAmount();
            this.ExecutionDate = dailyValue.DATE;
            this.State = OrderStatus.Executed;
            this.ExecutionDate = dailyValue.DATE;
         }
         else
         {
            // Detect wether buy order has to be executed according to the daily value trend
            switch (dailyValue.getOHLCType())
            {
               // The up trend indicates the stock reaches its low before reaching its high
               case OHLCType.UpTrend:
               case OHLCType.BottomUpTrend:
               case OHLCType.TopUpTrend:
                  // Update the benchmark with the new low value if lower than previous benchmark
                  Benchmark = Math.Min(dailyValue.LOW, Benchmark);

                  if (dailyValue.HIGH >= TargetLimit)
                  {
                     // Execute Order
                     this.Value = TargetLimit;
                     CalculateNumberAndFeeFromAmount();
                     this.ExecutionDate = dailyValue.DATE;
                     this.State = OrderStatus.Executed;
                     this.ExecutionDate = dailyValue.DATE;
                  }
                  break;
               // The down trend indicates the stock reaches its high before reaching its low
               case OHLCType.TopDownTrend:
               case OHLCType.BottomDownTrend:
               case OHLCType.DownTrend:

                  if (dailyValue.HIGH >= TargetLimit)  // Target limit was between Open and High
                  {
                     // Execute Order
                     this.Value = TargetLimit;
                     CalculateNumberAndFeeFromAmount();
                     this.ExecutionDate = dailyValue.DATE;
                     this.State = OrderStatus.Executed;
                     this.ExecutionDate = dailyValue.DATE;
                  }
                  else
                  {   // Update the benchmark with the new low value if lower than previous benchmark
                     Benchmark = Math.Min(dailyValue.LOW, Benchmark);

                     if (dailyValue.CLOSE >= TargetLimit)    // Check if close is higher than target
                     {
                        // Execute Order
                        this.Value = TargetLimit;
                        CalculateNumberAndFeeFromAmount();
                        this.ExecutionDate = dailyValue.DATE;
                        this.State = OrderStatus.Executed;
                        this.ExecutionDate = dailyValue.DATE;
                     }
                  }
                  break;
               default:
                  break;
            }
         }
      }
      private void ProcessSellTrailingOrder(StockDailyValue dailyValue)
      {
         // Deal with stock market open fix
         if (dailyValue.OPEN <= TargetLimit)
         {
            this.Value = dailyValue.OPEN;
            Fee = CalculateFee(this.Number, this.Value);
            this.ExecutionDate = dailyValue.DATE;
            this.State = OrderStatus.Executed;
         }
         else
         {
            switch (dailyValue.getOHLCType())
            {
               // The up trend indicates the stock reaches its low before reaching its high
               case OHLCType.UpTrend:
               case OHLCType.BottomUpTrend:
               case OHLCType.TopUpTrend:
                  if (dailyValue.OPEN <= TargetLimit)
                  {
                     this.Value = dailyValue.OPEN;
                     Fee = CalculateFee(this.Number, this.Value);
                     this.ExecutionDate = dailyValue.DATE;
                     this.State = OrderStatus.Executed;
                  }
                  else
                  {
                     if (dailyValue.LOW <= TargetLimit)
                     {
                        this.Value = TargetLimit;
                        Fee = CalculateFee(this.Number, this.Value);
                        this.ExecutionDate = dailyValue.DATE;
                        this.State = OrderStatus.Executed;
                     }
                     else
                     {
                        // The stock reaches its high before it closes so update the benchmark
                        Benchmark = Math.Max(dailyValue.HIGH, Benchmark);

                        // Check is close is lower than target
                        if (dailyValue.CLOSE <= TargetLimit)
                        {
                           // Create new sell order
                           this.Value = TargetLimit;
                           Fee = CalculateFee(this.Number, this.Value);
                           this.ExecutionDate = dailyValue.DATE;
                           this.State = OrderStatus.Executed;
                        }
                     }
                  }
                  break;
               // The down trend indicates the stock reaches its high before reaching its low
               case OHLCType.TopDownTrend:
               case OHLCType.BottomDownTrend:
               case OHLCType.DownTrend:
                  Benchmark = Math.Max(dailyValue.HIGH, Benchmark);

                  if (dailyValue.LOW <= TargetLimit)
                  {
                     this.Value = TargetLimit;
                     Fee = CalculateFee(this.Number, this.Value);
                     this.ExecutionDate = dailyValue.DATE;
                     this.State = OrderStatus.Executed;
                  }
                  break;
               default:
                  break;
            }
         }
      }
      #endregion
      #region Order parsing
      public static StockOrder CreateFromStockBrokers(string line, ref string errorMsg)
      {
         StockOrder stockOrder = new StockOrder();
         try
         {
            string[] fields = line.Split('|');

            // Find order date
            stockOrder.ExecutionDate = DateTime.Parse(fields[0]);
            stockOrder.CreationDate = DateTime.Parse(fields[0]);

            // Find order type
            if (fields[1] == "Buy")
            {
               stockOrder.Type = StockOrder.OrderType.BuyAtLimit;
            }
            else if (fields[1] == "Sell")
            {
               stockOrder.Type = StockOrder.OrderType.SellAtLimit;
            }
            else
            {
               errorMsg = "Invalid order type: " + fields[1];
               return null;
            }

            // Find Stock Name
            stockOrder.StockName = fields[2];

            // Find number of stocks
            stockOrder.Number = int.Parse(fields[4]);

            // Find stock unit value
            stockOrder.Value = float.Parse(fields[5], StockAnalyzerApp.Global.EnglishCulture) / 100.0f;

            // Find Total order value and fee
            float totalOrderValue = 0.0f;
            if (stockOrder.IsBuyOrder())
            {
               totalOrderValue = float.Parse(fields[6], StockAnalyzerApp.Global.EnglishCulture);
               stockOrder.Fee = totalOrderValue - stockOrder.Number * stockOrder.Value;
            }
            else
            {
               totalOrderValue = float.Parse(fields[7], StockAnalyzerApp.Global.EnglishCulture);
               stockOrder.Fee = stockOrder.Number * stockOrder.Value - totalOrderValue;
            }
         }
         catch (System.Exception e)
         {
            errorMsg = e.Message;
            stockOrder = null;
         }
         return stockOrder;
      }
      public static StockOrder CreateFromCAPCA(string line, ref string errorMsg)
      {
         StockOrder stockOrder = new StockOrder();
         try
         {
            string[] fields = line.Split('|');

            // Find order date
            stockOrder.ExecutionDate = DateTime.Parse(fields[0]);
            stockOrder.CreationDate = DateTime.Parse(fields[0]);

            // Find order type
            if (fields[1] == "Buy")
            {
               stockOrder.Type = StockOrder.OrderType.BuyAtLimit;
            }
            else if (fields[1] == "Sell")
            {
               stockOrder.Type = StockOrder.OrderType.SellAtLimit;
            }
            else
            {
               errorMsg = "Invalid order type: " + fields[1];
               return null;
            }


            // Find number of stocks
            stockOrder.Number = int.Parse(fields[2].Replace(" ", ""));

            // Find Stock Name
            string stockName = fields[3].Substring(fields[3].IndexOf(' ') + 1);
            int endIndex = stockName.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-' }) - 1;
            stockOrder.StockName = stockName.Substring(0, endIndex);

            // Find stock unit value
            stockOrder.Value = float.Parse(fields[4], StockAnalyzerApp.Global.EnglishCulture);

            // There'sno fee in the report :-(
            stockOrder.Fee = 0.0f;
         }
         catch (System.Exception e)
         {
            errorMsg = e.Message;
            stockOrder = null;
         }
         return stockOrder;
      }
      #endregion

      public object Clone()
      {
         return this.MemberwiseClone();
      }

   }
}